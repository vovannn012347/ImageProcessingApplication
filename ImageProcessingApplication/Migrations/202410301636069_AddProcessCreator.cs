namespace ImageProcessingApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProcessCreator : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProcessOperations", "CreatorUserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.ProcessOperations", "CreatorUserId");
            AddForeignKey("dbo.ProcessOperations", "CreatorUserId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProcessOperations", "CreatorUserId", "dbo.AspNetUsers");
            DropIndex("dbo.ProcessOperations", new[] { "CreatorUserId" });
            DropColumn("dbo.ProcessOperations", "CreatorUserId");
        }
    }
}
