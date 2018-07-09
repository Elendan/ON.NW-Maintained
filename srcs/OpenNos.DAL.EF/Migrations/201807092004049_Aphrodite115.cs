namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite115 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LevelUpRewards",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Level = c.Short(),
                        JobLevel = c.Short(),
                        HeroLvl = c.Short(),
                        Vnum = c.Short(nullable: false),
                        Amount = c.Short(nullable: false),
                        IsMate = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.LevelUpRewards");
        }
    }
}
