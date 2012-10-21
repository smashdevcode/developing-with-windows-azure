using DevelopingWithWindowsAzure.Shared.Entities;
using System;
using System.Collections.Generic;

namespace DevelopingWithWindowsAzure.Shared.Data
{
	public interface IRepository : IDisposable
	{
		List<Video> GetVideos();
		List<Video> GetVideos(Enums.VideoStatus videoStatus);
		Video GetVideo(int videoID);
		void InsertOrUpdateVideo(Video video);
		void DeleteVideo(int videoID);
		void DeleteVideo(Video video);
		List<VideoAsset> GetVideoAssets(int videoID);
		void InsertOrUpdateVideoAsset(VideoAsset videoAsset);
	}
}
