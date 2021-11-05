namespace GeneralStoreAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HashStack : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Transactions", "CombinedProductSKUString", c => c.String(nullable: false));
            AddColumn("dbo.Transactions", "CombinedItemCountString", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Transactions", "CombinedItemCountString");
            DropColumn("dbo.Transactions", "CombinedProductSKUString");
        }
    }
}
