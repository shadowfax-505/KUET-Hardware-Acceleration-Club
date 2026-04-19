using KUETHardwareAccelerationClub.Models;
using KUETHardwareAccelerationClub.Services;
using Microsoft.AspNetCore.Mvc;

namespace KUETHardwareAccelerationClub.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Auth() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(RegisterInput input)
    {
        var result = ClubRepository.RegisterMember(input);
        TempData["AuthMessage"] = result.message;
        TempData["AuthMessageType"] = result.success ? "success" : "error";

        if (result.success)
        {
            HttpContext.Session.SetString("CurrentUserEmail", input.Email.Trim().ToLowerInvariant());
        }

        return RedirectToAction(nameof(Auth));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginInput input)
    {
        var valid = ClubRepository.ValidateLogin(input);
        if (!valid)
        {
            TempData["AuthMessage"] = "Invalid email or password.";
            TempData["AuthMessageType"] = "error";
            return RedirectToAction(nameof(Auth));
        }

        HttpContext.Session.SetString("CurrentUserEmail", input.Email.Trim().ToLowerInvariant());
        TempData["AuthMessage"] = "Login successful.";
        TempData["AuthMessageType"] = "success";
        return RedirectToAction(nameof(Auth));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("CurrentUserEmail");
        return RedirectToAction("Index", "Home");
    }
}