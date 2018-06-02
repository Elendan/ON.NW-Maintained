namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite111 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RollGeneratedItem", "ItemGeneratedAmount", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RollGeneratedItem", "ItemGeneratedAmount");
        }
    }
}
