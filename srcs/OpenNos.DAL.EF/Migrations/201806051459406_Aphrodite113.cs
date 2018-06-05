namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite113 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UpgradeLog", "Success", c => c.Boolean(nullable: false));
            AddColumn("dbo.UpgradeLog", "ItemVnum", c => c.Short(nullable: false));
            AddColumn("dbo.UpgradeLog", "ItemName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UpgradeLog", "ItemName");
            DropColumn("dbo.UpgradeLog", "ItemVnum");
            DropColumn("dbo.UpgradeLog", "Success");
        }
    }
}
