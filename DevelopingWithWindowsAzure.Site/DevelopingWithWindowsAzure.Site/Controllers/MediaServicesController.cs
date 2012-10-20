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
			var assets = new MediaServices().GetAssets().OrderByDescending(a => a.Created).ToList();
            return View(assets);
        }
		public ActionResult AssetDetails(string assetID)
		{
			var asset = new MediaServices().GetAsset(assetID);
			return View(asset);
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
		public ActionResult JobDetails(string jobID)
		{
			var job = new MediaServices().GetJob(jobID);
			return View(job);
		}
		public ActionResult TaskDetails(string jobID, string taskID)
		{
			var job = new MediaServices().GetJob(jobID);
			var task = job.Tasks.Where(t => t.Id == taskID).First();
			ViewBag.JobID = jobID;
			return View(task);
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
