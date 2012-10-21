using DevelopingWithWindowsAzure.Shared.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DevelopingWithWindowsAzure.Site.Controllers
{
    public class MediaServicesAPIController : ApiController
    {
		public MediaServices MediaServices { get; set; }

		public MediaServicesAPIController()
		{
			this.MediaServices = new MediaServices();
		}

		public string GetAssetLocator(string assetID, string fileID)
		{
			var asset = this.MediaServices.GetAsset(assetID);
			var file = asset.Files.Where(f => f.Id == fileID).First();
			return this.MediaServices.GetAssetSasUrl(asset, file);
		}
    }
}
