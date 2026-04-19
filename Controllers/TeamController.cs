using KUETHardwareAccelerationClub.Services;
using Microsoft.AspNetCore.Mvc;

namespace KUETHardwareAccelerationClub.Controllers;

public class TeamController : Controller
{
    [HttpGet]
    public IActionResult Executives() => View(ClubRepository.GetExecutives());

    [HttpGet]
    public IActionResult Advisors() => View(ClubRepository.GetAdvisors());
}