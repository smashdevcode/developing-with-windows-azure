using DevelopingWithWindowsAzure.Shared.Data;
using DevelopingWithWindowsAzure.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopingWithWindowsAzure.Shared.Helper
{
	public class VideoHelper
	{
		private IRepository _repository;

		public VideoHelper(IRepository repository)
		{
			_repository = repository;
		}

		public void SaveVideo(Video video)
		{
			// JCTODO save to the database
			// JCTODO save binary data as a blob
		}
	}
}
