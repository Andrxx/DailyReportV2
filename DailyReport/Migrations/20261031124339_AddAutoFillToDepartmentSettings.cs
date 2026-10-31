using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoFillToDepartmentSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoFillFromPrevious",
                table: "DepartmentSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AutoFillSource",
                table: "DepartmentSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoFillFromPrevious",
                table: "DepartmentSettings");

            migrationBuilder.DropColumn(
                name: "AutoFillSource",
                table: "DepartmentSettings");
        }
    }
}
