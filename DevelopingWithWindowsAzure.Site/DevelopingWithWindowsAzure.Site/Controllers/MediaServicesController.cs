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
        public ActionResult Assets()
        {
			var assets = new MediaServices().GetAssets();
            return View(assets);
        }
		public ActionResult ContentKeys()
		{
			var contentKeys = new MediaServices().GetContentKeys();
			return View(contentKeys);
		}
		public ActionResult Files()
		{
			var files = new MediaServices().GetFiles();
			return View(files);
		}
		public ActionResult Jobs()
		{
			var jobs = new MediaServices().GetJobs();
			return View(jobs);
		}
		public ActionResult Locators()
		{
			var locators = new MediaServices().GetLocators();
			return View(locators);
		}
		public ActionResult MediaProcessors()
		{
			var mediaProcessors = new MediaServices().GetMediaProcessors();
			return View(mediaProcessors);
		}
	}
}
