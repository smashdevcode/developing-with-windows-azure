using DevelopingWithWindowsAzure.Shared.Data;
using DevelopingWithWindowsAzure.Shared.Entities;
using DevelopingWithWindowsAzure.Shared.Media;
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
		public ActionResult Upload()
		{
			return View(new Video());
		}
		[HttpPost]
		public ActionResult Upload(Video video, HttpPostedFileBase file)
		{
			// JCTODO setup validations for the file???
			// file types???
			// size???
			// other???

			// set properties on the video object
			// JCTODO use method on the entity that accepts HttpPostedFileBase instance???
			video.FileName = file.FileName;
			video.FileData = file.InputStream;
			video.AddedOn = DateTime.UtcNow;
			video.VideoStatusEnum = Shared.Enums.VideoStatus.Uploaded;

			var videoHelper = new VideoProcessor(_repository);
			videoHelper.SaveVideo(video);

			return RedirectToAction("Index");
		}
	}
}
