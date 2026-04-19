using KUETHardwareAccelerationClub.Models;
using KUETHardwareAccelerationClub.Services;
using Microsoft.AspNetCore.Mvc;

namespace KUETHardwareAccelerationClub.Controllers;

public class ContactController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Submit(ContactSubmission submission)
    {
        var result = ClubRepository.SubmitContact(submission);
        TempData["ContactMessage"] = result.message;
        TempData["ContactMessageType"] = result.success ? "success" : "error";
        return RedirectToAction(nameof(Index));
    }
}