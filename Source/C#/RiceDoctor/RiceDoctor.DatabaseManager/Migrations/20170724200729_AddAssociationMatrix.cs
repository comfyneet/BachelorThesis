using Microsoft.EntityFrameworkCore.Migrations;

namespace RiceDoctor.DatabaseManager.Migrations
{
    public partial class AddAssociationMatrix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Associations",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssociationScore = table.Column<double>(nullable: false),
                    Term1 = table.Column<string>(nullable: true),
                    Term2 = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Associations", x => x.Id); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Associations");
        }
    }
}