using DevelopingWithWindowsAzure.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;

namespace DevelopingWithWindowsAzure.Shared.Data
{
	internal class Context : DbContext
	{
		public Context() : base("name=Database")
		{
		}

		public DbSet<Video> Videos { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

			var videoEntity = modelBuilder.Entity<Video>();
			videoEntity.Property(v => v.FileName).HasMaxLength(255).IsRequired();
			videoEntity.Property(v => v.Title).HasMaxLength(100).IsRequired();
			videoEntity.Ignore(v => v.VideoStatusEnum);
			videoEntity.Ignore(v => v.FileData);
		}
	}
}
