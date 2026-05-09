using KUETHardwareAccelerationClub.Models;
using KUETHardwareAccelerationClub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KUETHardwareAccelerationClub.Controllers;

public class EventsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var currentUser = User.Identity?.IsAuthenticated == true
            ? (User.Identity?.Name ?? string.Empty)
            : string.Empty;

        var model = new EventsPageViewModel
        {
            Events = ClubRepository.GetEvents(),
            RegisteredEventIds = ClubRepository.GetRegisteredEventIds(currentUser)
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Member,Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult Register(int eventId)
    {
        var currentUser = User.Identity?.IsAuthenticated == true
            ? (User.Identity?.Name ?? string.Empty)
            : string.Empty;
        var result = ClubRepository.RegisterForEvent(eventId, currentUser);
        TempData["EventMessage"] = result.message;
        TempData["EventMessageType"] = result.success ? "success" : "error";
        return RedirectToAction(nameof(Index));
    }
}