using KUETHardwareAccelerationClub.Services;
using Microsoft.AspNetCore.Mvc;

namespace KUETHardwareAccelerationClub.Controllers;

public class TeamController : Controller
{
    [HttpGet]
    public IActionResult MeetTheTeam() => View(ClubRepository.GetExecutives());

    [HttpGet]
    public IActionResult Executives() => RedirectToAction(nameof(MeetTheTeam));

    [HttpGet]
    public IActionResult Advisors() => View(ClubRepository.GetAdvisors());
}