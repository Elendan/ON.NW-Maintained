namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite116 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.LevelUpRewards", "IsMate", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.LevelUpRewards", "IsMate", c => c.Boolean(nullable: false));
        }
    }
}
