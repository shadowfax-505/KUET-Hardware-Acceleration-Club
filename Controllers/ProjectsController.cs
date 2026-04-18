using System.Web.Mvc;
using KUETHardwareAccelerationClub.Services;

namespace KUETHardwareAccelerationClub.Controllers
{
    public class ProjectsController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            var currentUserEmail = Session["CurrentUserEmail"] as string;
            var model = ClubRepository.GetProjectsPage(currentUserEmail);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(int projectId, string content)
        {
            var currentUserEmail = Session["CurrentUserEmail"] as string;
            if (string.IsNullOrWhiteSpace(currentUserEmail))
            {
                TempData["ProjectMessage"] = "Please login to post comments.";
                return RedirectToAction("Auth", "Account");
            }

            ClubRepository.AddComment(projectId, currentUserEmail, content);
            TempData["ProjectMessage"] = "Comment posted.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReportComment(int commentId, string reason)
        {
            var currentUserEmail = Session["CurrentUserEmail"] as string;
            var result = ClubRepository.ReportComment(commentId, currentUserEmail, reason);
            TempData["ProjectMessage"] = result.message;
            return RedirectToAction("Index");
        }
    }
}
