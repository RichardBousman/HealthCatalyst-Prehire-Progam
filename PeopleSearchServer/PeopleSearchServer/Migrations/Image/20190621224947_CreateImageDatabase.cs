using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PeopleSearchServer.Migrations.Image
{
    public partial class CreateImageDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Image",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Jpeg = table.Column<byte[]>(nullable: true),
                    NumberPeopleAssignedTo = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Image", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Image");
        }
    }
}
