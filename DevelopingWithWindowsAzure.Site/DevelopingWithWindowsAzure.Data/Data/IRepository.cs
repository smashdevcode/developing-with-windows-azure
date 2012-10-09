using DevelopingWithWindowsAzure.Shared.Entities;
using System;
using System.Collections.Generic;

namespace DevelopingWithWindowsAzure.Shared.Data
{
	public interface IRepository : IDisposable
	{
		void DeleteVideo(int videoID);
		Video GetVideo(int videoID);
		List<Video> GetVideos();
		void InsertOrUpdateVideo(Video video);
	}
}
