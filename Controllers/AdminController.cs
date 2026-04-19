using KUETHardwareAccelerationClub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KUETHardwareAccelerationClub.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(ClubRepository.GetAdminDashboard());
    }
}