using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopingWithWindowsAzure.Shared.Entities
{
	public class Video
	{
		public int VideoID { get; set; }
		public string FileName { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime AddedOn { get; set; }
	}
}
