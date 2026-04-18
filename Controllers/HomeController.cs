using System.Web.Mvc;
using KUETHardwareAccelerationClub.Services;

namespace KUETHardwareAccelerationClub.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var model = ClubRepository.GetHomeContent();
            return View(model);
        }
    }
}
