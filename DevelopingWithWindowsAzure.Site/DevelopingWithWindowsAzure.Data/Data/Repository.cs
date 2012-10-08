using DevelopingWithWindowsAzure.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevelopingWithWindowsAzure.Shared.Data
{
	public class Repository : IDisposable
	{
		private Context _context;

		public Repository()
		{
			_context = new Context();
		}

		public List<Video> GetVideos()
		{
			return _context.Videos.ToList();
		}
		public Video GetVideo(int videoID)
		{
			return _context.Videos.Find(videoID);
		}
		// JCTODO change to SaveVideo??? how does add/edit differ???
		public void AddVideo(Video video)
		{
			_context.Videos.Add(video);
			_context.SaveChanges();
		}
		public void DeleteVideo(int videoID)
		{
			// JCTODO setup delete (need to do a get first???)
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
