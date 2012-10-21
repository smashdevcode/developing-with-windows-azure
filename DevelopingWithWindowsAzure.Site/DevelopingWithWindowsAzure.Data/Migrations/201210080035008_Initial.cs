namespace DevelopingWithWindowsAzure.Shared.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Video",
                c => new
                    {
                        VideoID = c.Int(nullable: false, identity: true),
                        FileName = c.String(nullable: false, maxLength: 255),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        AddedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.VideoID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Video");
        }
    }
}
