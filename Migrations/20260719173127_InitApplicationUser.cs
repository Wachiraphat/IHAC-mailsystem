using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalProject.Migrations
{
    public partial class AddEmails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmailReceiver = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Body = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    DateSent = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EmailSender = table.Column<string>(type: "TEXT", nullable: false),
                    ReadStatus = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Emails");
        }
    }
}
