namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite117 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LevelUpRewards", "MateLevel", c => c.Short());
            AddColumn("dbo.LevelUpRewards", "MateType", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LevelUpRewards", "MateType");
            DropColumn("dbo.LevelUpRewards", "MateLevel");
        }
    }
}
