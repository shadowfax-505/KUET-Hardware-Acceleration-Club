using KUETHardwareAccelerationClub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KUETHardwareAccelerationClub.Controllers;

public class ProjectsController : Controller
{
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
}