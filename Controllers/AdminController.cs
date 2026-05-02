using KUETHardwareAccelerationClub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace KUETHardwareAccelerationClub.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(ClubRepository.GetAdminDashboard());
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

    private static string Csv(string value)
    {
        var safe = (value ?? string.Empty).Replace("\"", "\"\"");
        return $"\"{safe}\"";
    }
}