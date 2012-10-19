using DevelopingWithWindowsAzure.Shared.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DevelopingWithWindowsAzure.Site.Controllers
{
    public class MediaServicesController : Controller
    {
        public ActionResult Index()
        {
			var mediaServices = new MediaServices();
			var assets = mediaServices.GetAssets();
            return View(assets);
        }
    }
}
