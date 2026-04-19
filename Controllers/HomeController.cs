using KUETHardwareAccelerationClub.Models;
using KUETHardwareAccelerationClub.Services;
using Microsoft.AspNetCore.Mvc;

namespace KUETHardwareAccelerationClub.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var model = ClubRepository.GetHomeContent();
        return View(model);
    }

    [HttpGet]
    public IActionResult About() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Subscribe(string email)
    {
        var result = ClubRepository.SubscribeNewsletter(email);
        TempData["HomeMessage"] = result.message;
        TempData["HomeMessageType"] = result.success ? "success" : "error";
        return RedirectToAction(nameof(Index));
    }
}