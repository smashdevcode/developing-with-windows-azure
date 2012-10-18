using DevelopingWithWindowsAzure.Shared.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DevelopingWithWindowsAzure.Shared.Entities
{
	public class Video
	{
		public int VideoID { get; set; }
		public string FileName { get; set; }
		public string FileNameWithoutExtension
		{
			get
			{
				if (!string.IsNullOrEmpty(this.FileName))
					return System.IO.Path.GetFileNameWithoutExtension(this.FileName);
				else
					return null;
			}
		}
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime AddedOn { get; set; }
		public int VideoStatus { get; set; }
		public VideoStatus VideoStatusEnum
		{
			get { return (VideoStatus)this.VideoStatus; }
			set { this.VideoStatus = (int)value; }
		}
		public Stream FileData { get; set; }
	}
}
