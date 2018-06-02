namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite110 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.RollGeneratedItem", "ItemGeneratedAmount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RollGeneratedItem", "ItemGeneratedAmount", c => c.Byte(nullable: false));
        }
    }
}
