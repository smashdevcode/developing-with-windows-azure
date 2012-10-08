using DevelopingWithWindowsAzure.Shared.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DevelopingWithWindowsAzure.Site.Controllers
{
	public class VideosController : Controller
	{
		private Repository _repository = new Repository();

		public ActionResult Index()
		{
			return View(_repository.GetVideos());
		}
	}
}
