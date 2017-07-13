using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RiceDoctor.DatabaseManager.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Websites",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Websites", x => x.Id); });

            migrationBuilder.CreateTable(
                "Articles",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    RetrievedDate = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    WebsiteId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                    table.ForeignKey(
                        "FK_Articles_Websites_WebsiteId",
                        x => x.WebsiteId,
                        "Websites",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "Categories",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArticleXPath = table.Column<string>(nullable: true),
                    ContentXPath = table.Column<string>(nullable: true),
                    TitleXPath = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    WebsiteId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        "FK_Categories_Websites_WebsiteId",
                        x => x.WebsiteId,
                        "Websites",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_Articles_WebsiteId",
                "Articles",
                "WebsiteId");

            migrationBuilder.CreateIndex(
                "IX_Categories_WebsiteId",
                "Categories",
                "WebsiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Articles");

            migrationBuilder.DropTable(
                "Categories");

            migrationBuilder.DropTable(
                "Websites");
        }
    }
}