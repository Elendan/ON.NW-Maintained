namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite119 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AntiBotLog",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CharacterId = c.Long(nullable: false),
                        CharacterName = c.String(),
                        Timeout = c.Boolean(nullable: false),
                        DateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AntiBotLog");
        }
    }
}
