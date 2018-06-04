namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite112 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExchangeLog",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AccountId = c.Long(nullable: false),
                        CharacterId = c.Long(nullable: false),
                        CharacterName = c.String(),
                        TargetAccountId = c.Long(nullable: false),
                        TargetCharacterId = c.Long(nullable: false),
                        TargetCharacterName = c.String(),
                        ItemVnum = c.Short(nullable: false),
                        ItemAmount = c.Short(nullable: false),
                        Gold = c.Long(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UpgradeLog",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AccountId = c.Long(nullable: false),
                        CharacterId = c.Long(nullable: false),
                        CharacterName = c.String(),
                        UpgradeType = c.String(),
                        HasAmulet = c.Boolean(),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UpgradeLog");
            DropTable("dbo.ExchangeLog");
        }
    }
}
