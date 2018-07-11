namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite109 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItemInstance", "Agility", c => c.Byte(nullable: false));
            AddColumn("dbo.ItemInstance", "PartnerSkill1", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "PartnerSkill2", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "PartnerSkill3", c => c.Short(nullable: false));
            AddColumn("dbo.ItemInstance", "SkillRank1", c => c.Byte(nullable: false));
            AddColumn("dbo.ItemInstance", "SkillRank2", c => c.Byte(nullable: false));
            AddColumn("dbo.ItemInstance", "SkillRank3", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItemInstance", "SkillRank3");
            DropColumn("dbo.ItemInstance", "SkillRank2");
            DropColumn("dbo.ItemInstance", "SkillRank1");
            DropColumn("dbo.ItemInstance", "PartnerSkill3");
            DropColumn("dbo.ItemInstance", "PartnerSkill2");
            DropColumn("dbo.ItemInstance", "PartnerSkill1");
            DropColumn("dbo.ItemInstance", "Agility");
        }
    }
}
