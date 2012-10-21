using DevelopingWithWindowsAzure.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopingWithWindowsAzure.Shared.Entities
{
	public class VideoAsset
	{
		public int VideoAssetID { get; set; }
		public int VideoID { get; set; }
		public Video Video { get; set; }
		public int FileType { get; set; }
		public FileType FileTypeEnum
		{
			get { return (FileType)this.FileType; }
			set { this.FileType = (int)value; }
		}
		public string FileTypeExtension
		{
			get
			{
				return FileTypeHelper.GetFileTypeExtension(this.FileTypeEnum);
			}
		}
		public string MediaServicesAssetID { get; set; }
	}
}
