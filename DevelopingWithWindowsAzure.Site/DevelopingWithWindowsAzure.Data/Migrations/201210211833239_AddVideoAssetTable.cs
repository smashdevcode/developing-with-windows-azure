namespace DevelopingWithWindowsAzure.Shared.Migrations
{
	using System;
	using System.Data.Entity.Migrations;

	public partial class AddVideoAssetTable : DbMigration
	{
		public override void Up()
		{
			CreateTable(
				"dbo.VideoAsset",
				c => new
					{
						VideoAssetID = c.Int(nullable: false, identity: true),
						VideoID = c.Int(nullable: false),
						FileType = c.Int(nullable: false),
						MediaServicesAssetID = c.String(nullable: false, maxLength: 50),
					})
				.PrimaryKey(t => t.VideoAssetID)
				.ForeignKey("dbo.Video", t => t.VideoID, cascadeDelete: true)
				.Index(t => t.VideoID);

			AddColumn("dbo.Video", "MediaServicesJobID", c => c.String(maxLength: 50));
		}

		public override void Down()
		{
			DropIndex("dbo.VideoAsset", new[] { "VideoID" });
			DropForeignKey("dbo.VideoAsset", "VideoID", "dbo.Video");
			DropColumn("dbo.Video", "MediaServicesJobID");
			DropTable("dbo.VideoAsset");
		}
	}
}
