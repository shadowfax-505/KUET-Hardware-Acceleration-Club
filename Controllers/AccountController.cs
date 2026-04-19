using KUETHardwareAccelerationClub.Models;
using KUETHardwareAccelerationClub.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KUETHardwareAccelerationClub.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Auth() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterInput input)
    {
        var email = input.Email.Trim().ToLowerInvariant();
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            TempData["AuthMessage"] = "This email is already registered.";
            TempData["AuthMessageType"] = "error";
            return RedirectToAction(nameof(Auth));
        }

        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, input.Password.Trim());
        if (!createResult.Succeeded)
        {
            TempData["AuthMessage"] = string.Join(" ", createResult.Errors.Select(e => e.Description));
            TempData["AuthMessageType"] = "error";
            return RedirectToAction(nameof(Auth));
        }

        var memberResult = ClubRepository.RegisterMember(input);
        if (!memberResult.success)
        {
            await _userManager.DeleteAsync(user);
            TempData["AuthMessage"] = memberResult.message;
            TempData["AuthMessageType"] = "error";
            return RedirectToAction(nameof(Auth));
        }

        await _userManager.AddToRoleAsync(user, "Member");

        await _signInManager.SignInAsync(user, isPersistent: false);

        TempData["AuthMessage"] = "Registration successful.";
        TempData["AuthMessageType"] = "success";
        return RedirectToAction(nameof(Auth));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInput input)
    {
        var email = input.Email.Trim().ToLowerInvariant();
        var result = await _signInManager.PasswordSignInAsync(email, input.Password.Trim(), isPersistent: false, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            TempData["AuthMessage"] = "Invalid email or password.";
            TempData["AuthMessageType"] = "error";
            return RedirectToAction(nameof(Auth));
        }

        TempData["AuthMessage"] = "Login successful.";
        TempData["AuthMessageType"] = "success";
        return RedirectToAction(nameof(Auth));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}