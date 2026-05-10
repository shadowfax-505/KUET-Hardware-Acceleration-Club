using KUETHardwareAccelerationClub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KUETHardwareAccelerationClub.Controllers;

public class ProjectsController : Controller
{
    private readonly IWebHostEnvironment _env;

    public ProjectsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var currentUser = User.Identity?.IsAuthenticated == true
            ? (User.Identity?.Name ?? string.Empty)
            : string.Empty;
        var model = ClubRepository.GetProjectsPage(currentUser);
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Member,Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult AddComment(int projectId, string content)
    {
        var currentUser = User.Identity?.IsAuthenticated == true
            ? (User.Identity?.Name ?? string.Empty)
            : string.Empty;
        if (!ClubRepository.IsMember(currentUser))
        {
            TempData["ProjectMessage"] = "Only registered members can comment.";
            TempData["ProjectMessageType"] = "error";
            return RedirectToAction(nameof(Index));
        }

        ClubRepository.AddComment(projectId, currentUser, content);
        TempData["ProjectMessage"] = "Comment posted successfully.";
        TempData["ProjectMessageType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Member,Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult ReportComment(int commentId, string reason)
    {
        var currentUser = User.Identity?.IsAuthenticated == true
            ? (User.Identity?.Name ?? string.Empty)
            : string.Empty;
        var result = ClubRepository.ReportComment(commentId, currentUser, reason);
        TempData["ProjectMessage"] = result.message;
        TempData["ProjectMessageType"] = result.success ? "success" : "error";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Member,Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitProject(string Title, string Summary, string Lead, string Stack, List<IFormFile>? Photos)
    {
        var currentUser = User.Identity?.IsAuthenticated == true
            ? (User.Identity?.Name ?? string.Empty)
            : string.Empty;

        if (!ClubRepository.IsMember(currentUser))
        {
            TempData["ProjectMessage"] = "Only registered members can submit projects.";
            TempData["ProjectMessageType"] = "error";
            return RedirectToAction(nameof(Index));
        }

        var (success, message, submissionId) = ClubRepository.CreateProjectSubmission(Title, Summary, Lead, Stack, currentUser);
        if (!success)
        {
            TempData["ProjectMessage"] = message;
            TempData["ProjectMessageType"] = "error";
            return RedirectToAction(nameof(Index));
        }

        if (Photos is not null && Photos.Count > 0)
        {
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "projects", "submissions", submissionId.ToString());
            Directory.CreateDirectory(uploadsRoot);
            foreach (var file in Photos)
            {
                if (file is null || file.Length == 0) continue;
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp", ".gif" };
                if (!allowed.Contains(ext)) continue;
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsRoot, fileName);
                using var stream = System.IO.File.Create(filePath);
                file.CopyTo(stream);
                var webPath = $"/uploads/projects/submissions/{submissionId}/{fileName}";
                ClubRepository.AddProjectSubmissionImage(submissionId, webPath);
            }
        }

        TempData["ProjectMessage"] = "Project submitted for review. Admin will publish after approval.";
        TempData["ProjectMessageType"] = "success";
        return RedirectToAction(nameof(Index));
    }
}