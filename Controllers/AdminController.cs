using KUETHardwareAccelerationClub.Models;
using KUETHardwareAccelerationClub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace KUETHardwareAccelerationClub.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AdminController(UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
    {
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await BuildDashboardModelAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DashboardSnapshot()
    {
        var people = await BuildPeopleSnapshotAsync();
        return Json(new
        {
            updatedAtUtc = DateTime.UtcNow.ToString("O"),
            people,
            memberCount = people.Count(item => item.Role == "Member"),
            adminCount = people.Count(item => item.Role == "Admin")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SendAnnouncement(AdminBroadcastInput input)
    {
        var members = ClubRepository.GetMemberDirectory();
        var allowedRecipients = members.Select(member => member.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var audience = input.Audience ?? string.Empty;
        var selectedInputRecipients = input.SelectedRecipients ?? [];

        var selectedRecipients = audience.Equals("Custom", StringComparison.OrdinalIgnoreCase)
            ? selectedInputRecipients.Where(email => allowedRecipients.Contains(email.Trim())).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            : allowedRecipients.ToList();

        if (audience.Equals("Custom", StringComparison.OrdinalIgnoreCase) && selectedRecipients.Count == 0)
        {
            TempData["AdminMessage"] = "Choose at least one member for the custom broadcast.";
            TempData["AdminMessageType"] = "error";
            return RedirectToAction(nameof(Index));
        }

        var result = ClubRepository.SaveAnnouncement(input.Subject, input.Body, audience, selectedRecipients);
        TempData["AdminMessage"] = result.message;
        TempData["AdminMessageType"] = result.success ? "success" : "error";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadProfileImage(string profileKey, IFormFile image)
    {
        if (image is null || image.Length == 0)
        {
            TempData["AdminMessage"] = "Please choose an image to upload.";
            TempData["AdminMessageType"] = "error";
            return RedirectToAction(nameof(Index));
        }

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".webp", ".gif" };
        if (!allowedExtensions.Contains(extension))
        {
            TempData["AdminMessage"] = "Only PNG, JPG, JPEG, WEBP, and GIF files are allowed.";
            TempData["AdminMessageType"] = "error";
            return RedirectToAction(nameof(Index));
        }

        var uploadsRoot = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "team");
        Directory.CreateDirectory(uploadsRoot);

        var safeProfileKey = string.Concat((profileKey ?? string.Empty).Where(char.IsLetterOrDigit));
        if (string.IsNullOrWhiteSpace(safeProfileKey))
        {
            TempData["AdminMessage"] = "Invalid profile target.";
            TempData["AdminMessageType"] = "error";
            return RedirectToAction(nameof(Index));
        }

        var fileName = $"{safeProfileKey}-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var filePath = Path.Combine(uploadsRoot, fileName);
        await using (var stream = System.IO.File.Create(filePath))
        {
            await image.CopyToAsync(stream);
        }

        var webPath = $"/uploads/team/{fileName}";
        var saveResult = ClubRepository.SetProfileImageOverride(profileKey ?? string.Empty, webPath);
        TempData["AdminMessage"] = saveResult.message;
        TempData["AdminMessageType"] = saveResult.success ? "success" : "error";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult HideComment(int commentId)
    {
        var success = ClubRepository.SetCommentModerationStatus(commentId, "Hidden");
        TempData["AdminMessage"] = success ? "Comment hidden." : "Unable to hide comment.";
        TempData["AdminMessageType"] = success ? "success" : "error";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RestoreComment(int commentId)
    {
        var success = ClubRepository.SetCommentModerationStatus(commentId, "Visible");
        TempData["AdminMessage"] = success ? "Comment restored." : "Unable to restore comment.";
        TempData["AdminMessageType"] = success ? "success" : "error";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ClearReports(int commentId)
    {
        var success = ClubRepository.ResetCommentReports(commentId);
        TempData["AdminMessage"] = success ? "Reports cleared for comment." : "Unable to clear reports.";
        TempData["AdminMessageType"] = success ? "success" : "error";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveProfileDetails(string profileKey, string name, string email)
    {
        var result = ClubRepository.SetProfileOverride(profileKey, name, email);
        TempData["AdminMessage"] = result.message;
        TempData["AdminMessageType"] = result.success ? "success" : "error";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ExportContactsCsv()
    {
        var contacts = ClubRepository.GetAllContactsForExport();
        var builder = new StringBuilder();
        builder.AppendLine("Id,Name,Email,Topic,Message,SubmittedAtUtc");

        foreach (var item in contacts)
        {
            builder.AppendLine(string.Join(",",
                Csv(item.Id.ToString()),
                Csv(item.Name),
                Csv(item.Email),
                Csv(item.Topic),
                Csv(item.Message),
                Csv(item.SubmittedAtUtc.ToString("O"))));
        }

        var bytes = Encoding.UTF8.GetBytes(builder.ToString());
        var fileName = $"contacts-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return File(bytes, "text/csv", fileName);
    }

    private async Task<AdminDashboardViewModel> BuildDashboardModelAsync()
    {
        var model = ClubRepository.GetAdminDashboard();
        model.People = await BuildPeopleSnapshotAsync();
        model.LiveUpdatedAtUtc = DateTime.UtcNow.ToString("O");
        return model;
    }

    private async Task<List<DashboardPersonItem>> BuildPeopleSnapshotAsync()
    {
        var people = ClubRepository.GetMemberDirectory()
            .Select(member => new DashboardPersonItem
            {
                Role = "Member",
                Name = member.FullName,
                Email = member.Email,
                Department = member.Department,
                StudentId = member.StudentId
            })
            .ToList();

        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        people.AddRange(admins.Select(admin => new DashboardPersonItem
        {
            Role = "Admin",
            Name = admin.UserName ?? admin.Email ?? admin.Id,
            Email = admin.Email ?? admin.UserName ?? string.Empty,
            Department = "Admin",
            StudentId = "N/A"
        }));

        return people;
    }

    private static string Csv(string value)
    {
        var safe = (value ?? string.Empty).Replace("\"", "\"\"");
        return $"\"{safe}\"";
    }
}