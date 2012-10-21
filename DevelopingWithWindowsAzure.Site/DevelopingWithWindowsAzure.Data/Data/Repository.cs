using DevelopingWithWindowsAzure.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DevelopingWithWindowsAzure.Shared.Data
{
	public class Repository : IRepository
	{
		private Context _context;

		public Repository()
		{
			_context = new Context();
		}

		public List<Video> GetVideos()
		{
			return _context.Videos.OrderByDescending(v => v.AddedOn).ToList();
		}
		public Video GetVideo(int videoID)
		{
			return _context.Videos.Find(videoID);
		}
		public void InsertOrUpdateVideo(Video video)
		{
			_context.Entry(video).State = video.VideoID == 0 ? EntityState.Added : EntityState.Modified;
			_context.SaveChanges();
		}
		public void DeleteVideo(int videoID)
		{
			var video = new Video() { VideoID = videoID };
			_context.Videos.Attach(video);
			_context.Videos.Remove(video);
			_context.SaveChanges();
		}
		public void DeleteVideo(Video video)
		{
			_context.Videos.Remove(video);
			_context.SaveChanges();
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
