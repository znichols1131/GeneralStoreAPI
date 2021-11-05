namespace GeneralStoreAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MultipleProducts : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Transactions", "ProductSKU", "dbo.Products");
            DropIndex("dbo.Transactions", new[] { "ProductSKU" });
            AddColumn("dbo.Products", "Transaction_Id", c => c.Int());
            CreateIndex("dbo.Products", "Transaction_Id");
            AddForeignKey("dbo.Products", "Transaction_Id", "dbo.Transactions", "Id");
            DropColumn("dbo.Transactions", "ProductSKU");
            DropColumn("dbo.Transactions", "ItemCount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Transactions", "ItemCount", c => c.Int(nullable: false));
            AddColumn("dbo.Transactions", "ProductSKU", c => c.Int(nullable: false));
            DropForeignKey("dbo.Products", "Transaction_Id", "dbo.Transactions");
            DropIndex("dbo.Products", new[] { "Transaction_Id" });
            DropColumn("dbo.Products", "Transaction_Id");
            CreateIndex("dbo.Transactions", "ProductSKU");
            AddForeignKey("dbo.Transactions", "ProductSKU", "dbo.Products", "SKU", cascadeDelete: true);
        }
    }
}
