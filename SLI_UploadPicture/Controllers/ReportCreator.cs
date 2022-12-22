using Microsoft.AspNetCore.Mvc;

namespace SLI_UploadPicture.Controllers
{
    public class ReportCreator : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
