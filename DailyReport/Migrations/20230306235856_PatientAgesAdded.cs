using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class PatientAgesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Age",
                table: "OutcomingPatients",
                newName: "AgeYears");

            migrationBuilder.AddColumn<string>(
                name: "AgeMonth",
                table: "OutcomingPatients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ambulance",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ambulanceChildren",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "reject",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "rejectChildren",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sendToMO",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sendToMOChildren",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sumAdults",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sumAll",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sumChild",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgeMonth",
                table: "OutcomingPatients");

            migrationBuilder.DropColumn(
                name: "ambulance",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "ambulanceChildren",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "reject",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "rejectChildren",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "sendToMO",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "sendToMOChildren",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "sumAdults",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "sumAll",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "sumChild",
                table: "FinalReports");

            migrationBuilder.RenameColumn(
                name: "AgeYears",
                table: "OutcomingPatients",
                newName: "Age");
        }
    }
}
