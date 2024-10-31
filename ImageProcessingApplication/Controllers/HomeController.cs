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
    }
}
