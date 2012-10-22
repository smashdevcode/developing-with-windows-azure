namespace DevelopingWithWindowsAzure.Shared.Migrations
{
	using DevelopingWithWindowsAzure.Shared.Data;
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;

	public sealed class Configuration : DbMigrationsConfiguration<Context>
	{
		public Configuration()
		{
			// JCTODO had to change this to true because I mistakenly changed the name of a migration class
			AutomaticMigrationsEnabled = true;
		}

		protected override void Seed(Context context)
		{
			//  This method will be called after migrating to the latest version.

			//  You can use the DbSet<T>.AddOrUpdate() helper extension method 
			//  to avoid creating duplicate seed data. E.g.
			//
			//    context.People.AddOrUpdate(
			//      p => p.FullName,
			//      new Person { FullName = "Andrew Peters" },
			//      new Person { FullName = "Brice Lambson" },
			//      new Person { FullName = "Rowan Miller" }
			//    );
			//
		}
	}
}
