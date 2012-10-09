namespace DevelopingWithWindowsAzure.Shared.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddVideoStatusColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Video", "VideoStatus", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Video", "VideoStatus");
        }
    }
}
