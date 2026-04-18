using System.Web.Mvc;
using KUETHardwareAccelerationClub.Models;
using KUETHardwareAccelerationClub.Services;

namespace KUETHardwareAccelerationClub.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult Auth()
        {
            return View(new AuthPageViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterInput input)
        {
            if (string.IsNullOrWhiteSpace(input?.Email))
            {
                TempData["AuthMessage"] = "Please provide a valid email.";
                return RedirectToAction("Auth");
            }

            ClubRepository.RegisterMember(input.Email);
            Session["CurrentUserEmail"] = input.Email.Trim().ToLowerInvariant();
            TempData["AuthMessage"] = "Registration successful. Welcome to the club!";
            return RedirectToAction("Index", "Projects");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginInput input)
        {
            if (!ClubRepository.IsMember(input?.Email))
            {
                TempData["AuthMessage"] = "Login failed. You are not registered as a member yet.";
                return RedirectToAction("Auth");
            }

            Session["CurrentUserEmail"] = input.Email.Trim().ToLowerInvariant();
            TempData["AuthMessage"] = "Login successful.";
            return RedirectToAction("Index", "Projects");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Remove("CurrentUserEmail");
            TempData["AuthMessage"] = "Logged out successfully.";
            return RedirectToAction("Index", "Home");
        }
    }
}
