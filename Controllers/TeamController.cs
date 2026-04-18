using System.Web.Mvc;
using KUETHardwareAccelerationClub.Services;

namespace KUETHardwareAccelerationClub.Controllers
{
    public class TeamController : Controller
    {
        public ActionResult Executives()
        {
            var model = ClubRepository.GetExecutives();
            return View(model);
        }

        public ActionResult Advisors()
        {
            var model = ClubRepository.GetAdvisors();
            return View(model);
        }
    }
}
