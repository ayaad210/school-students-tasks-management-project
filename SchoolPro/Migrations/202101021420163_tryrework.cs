namespace SchoolPro.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tryrework : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Parents", newName: "Perants");
            RenameColumn(table: "dbo.Students", name: "Parent_Id", newName: "Perant_Id");
            RenameIndex(table: "dbo.Students", name: "IX_Parent_Id", newName: "IX_Perant_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Students", name: "IX_Perant_Id", newName: "IX_Parent_Id");
            RenameColumn(table: "dbo.Students", name: "Perant_Id", newName: "Parent_Id");
            RenameTable(name: "dbo.Perants", newName: "Parents");
        }
    }
}
