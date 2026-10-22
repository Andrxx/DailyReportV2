using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class AddMkbCodeToPatients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MkbCode",
                table: "Patients",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MkbCode",
                table: "Patients");
        }
    }
}
