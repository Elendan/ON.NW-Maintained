namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite118 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.LevelUpRewards", "Level", c => c.Short(nullable: false));
            AlterColumn("dbo.LevelUpRewards", "JobLevel", c => c.Short(nullable: false));
            AlterColumn("dbo.LevelUpRewards", "HeroLvl", c => c.Short(nullable: false));
            AlterColumn("dbo.LevelUpRewards", "IsMate", c => c.Boolean(nullable: false));
            AlterColumn("dbo.LevelUpRewards", "MateLevel", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.LevelUpRewards", "MateLevel", c => c.Short());
            AlterColumn("dbo.LevelUpRewards", "IsMate", c => c.Boolean());
            AlterColumn("dbo.LevelUpRewards", "HeroLvl", c => c.Short());
            AlterColumn("dbo.LevelUpRewards", "JobLevel", c => c.Short());
            AlterColumn("dbo.LevelUpRewards", "Level", c => c.Short());
        }
    }
}
