namespace TypeHierarchyModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BaseTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Thing1 = c.String(),
                        ThingA = c.String(),
                        OtherId = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OtherBaseTypes", t => t.OtherId, cascadeDelete: true)
                .Index(t => t.OtherId);
            
            CreateTable(
                "dbo.OtherBaseTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Something = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BaseTypes", "OtherId", "dbo.OtherBaseTypes");
            DropIndex("dbo.BaseTypes", new[] { "OtherId" });
            DropTable("dbo.OtherBaseTypes");
            DropTable("dbo.BaseTypes");
        }
    }
}
