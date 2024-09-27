using System;
using System.Web.Mvc;
using ImageProcessingApplication.Models;

namespace ImageProcessingApplication.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult ModelAction()
        {
            ViewBag.Title = "Model Action Page";

            string sampleString = "Sample " + DateTime.Now.Millisecond;

            return View(new SampleModel { SampleString = sampleString });
        }
    }
}
