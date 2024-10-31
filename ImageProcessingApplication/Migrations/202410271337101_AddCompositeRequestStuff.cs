namespace ImageProcessingApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCompositeRequestStuff : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AppSettings",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ImageSavePath = c.String(),
                        DefaultOperationLimit = c.Int(nullable: false),
                        UserMaxLimit = c.Int(nullable: false),
                        LimitRenewTime = c.Time(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProcessRequests",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ProcessOperation = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        ProcessState = c.String(),
                        ProcessResultJson = c.String(),
                        ParentRequestId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CompositeProcessRequests", t => t.ParentRequestId, cascadeDelete: true)
                .Index(t => t.ParentRequestId);
            
            CreateTable(
                "dbo.CompositeProcessRequests",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ProcessOperation = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                        ProcessState = c.String(),
                        ProcessHash = c.String(),
                        ProcessResultJson = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ImageFiles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FriendlyName = c.Int(nullable: false),
                        FilePath = c.Int(nullable: false),
                        TimeDue = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserLimits",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        OperationLimit = c.Int(nullable: false),
                        MaximumLimit = c.Int(nullable: false),
                        RenewTime = c.DateTime(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.OperationOrders",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ParamsMapping = c.String(),
                        NextOperationId = c.String(nullable: false, maxLength: 128),
                        PreviousOperationId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProcessOperations", t => t.NextOperationId)
                .ForeignKey("dbo.ProcessOperations", t => t.PreviousOperationId)
                .Index(t => t.NextOperationId)
                .Index(t => t.PreviousOperationId);
            
            CreateTable(
                "dbo.ProcessOperations",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CodeName = c.String(),
                        FriendlyName = c.String(),
                        FilePath = c.String(),
                        Metadata = c.String(),
                        InputParams = c.String(),
                        OutputParams = c.String(),
                        OperationsOrder = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OperationOrders", "PreviousOperationId", "dbo.ProcessOperations");
            DropForeignKey("dbo.OperationOrders", "NextOperationId", "dbo.ProcessOperations");
            DropForeignKey("dbo.UserLimits", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ProcessRequests", "ParentRequestId", "dbo.CompositeProcessRequests");
            DropIndex("dbo.OperationOrders", new[] { "PreviousOperationId" });
            DropIndex("dbo.OperationOrders", new[] { "NextOperationId" });
            DropIndex("dbo.UserLimits", new[] { "UserId" });
            DropIndex("dbo.ProcessRequests", new[] { "ParentRequestId" });
            DropTable("dbo.ProcessOperations");
            DropTable("dbo.OperationOrders");
            DropTable("dbo.UserLimits");
            DropTable("dbo.ImageFiles");
            DropTable("dbo.CompositeProcessRequests");
            DropTable("dbo.ProcessRequests");
            DropTable("dbo.AppSettings");
        }
    }
}
