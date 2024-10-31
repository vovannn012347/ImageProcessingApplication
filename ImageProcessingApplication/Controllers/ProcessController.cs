using System;
using System.Net;
using System.Web.Mvc;

using ImageProcessingApplication.Areas.Api.Controllers;
using ImageProcessingApplication.Models;

namespace ImageProcessingApplication.Controllers
{
    public class ProcessController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Processes Page";

            return View();
        }

        public ActionResult Algorithm(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Redirect("/Process/Index");
            }

            if(id == "sobel_processing_guid")
            {
                ViewBag.Title = "Algorithm";

                return View(new ProcessAlgorithmModel
                {
                    Id = id 
                });
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Algorithm not found.");
        }
    }
}
