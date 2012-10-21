using DevelopingWithWindowsAzure.Shared.Media;
using DevelopingWithWindowsAzure.Shared.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DevelopingWithWindowsAzure.Site.Controllers
{
    public class StorageController : Controller
    {
        public ActionResult Blobs()
        {
            return View(BlobStorage.GetBlobs(VideoProcessor.VIDEOS_CONTAINER));
        }
		public ActionResult BlobDelete(string fileName)
		{
			BlobStorage.DeleteBlob(VideoProcessor.VIDEOS_CONTAINER, fileName);
			return RedirectToAction("Blobs");
		}
		public ActionResult Containers()
		{
			return View(BlobStorage.GetContainers());
		}
		public ActionResult ContainerDelete(string containerName)
		{
			BlobStorage.DeleteContainer(containerName);
			return RedirectToAction("Containers");
		}
    }
}
