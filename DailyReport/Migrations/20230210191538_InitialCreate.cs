using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    depNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    existed = table.Column<int>(type: "INTEGER", nullable: false),
                    existedChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    income = table.Column<int>(type: "INTEGER", nullable: false),
                    incomeChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    outcome = table.Column<int>(type: "INTEGER", nullable: false),
                    outcomeChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    movedOutDep = table.Column<int>(type: "INTEGER", nullable: false),
                    movedOutDepChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    movedInDep = table.Column<int>(type: "INTEGER", nullable: false),
                    movedInDepChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    died = table.Column<int>(type: "INTEGER", nullable: false),
                    diedChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    present = table.Column<int>(type: "INTEGER", nullable: false),
                    presentChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    attachedToORIT = table.Column<int>(type: "INTEGER", nullable: false),
                    attachedToORITCildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    oIVL = table.Column<int>(type: "INTEGER", nullable: false),
                    oIVLChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    oNIVL = table.Column<int>(type: "INTEGER", nullable: false),
                    oNIVLChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    oNIVLVPO = table.Column<int>(type: "INTEGER", nullable: false),
                    oNIVLVPOChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    oNIVLMask = table.Column<int>(type: "INTEGER", nullable: false),
                    oNIVLMaskChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    oMask = table.Column<int>(type: "INTEGER", nullable: false),
                    oMaskChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    oBrease = table.Column<int>(type: "INTEGER", nullable: false),
                    oBreaseChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    pregnant = table.Column<int>(type: "INTEGER", nullable: false),
                    pregnantChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    restZone = table.Column<int>(type: "INTEGER", nullable: false),
                    restZoneChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    outRegions = table.Column<int>(type: "INTEGER", nullable: false),
                    outRegionsChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    forein = table.Column<int>(type: "INTEGER", nullable: false),
                    foreinChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    LNRDNR = table.Column<int>(name: "LNR_DNR", type: "INTEGER", nullable: false),
                    LNRDNRChildrens = table.Column<int>(name: "LNR_DNRChildrens", type: "INTEGER", nullable: false),
                    incomeHospital = table.Column<int>(type: "INTEGER", nullable: false),
                    incomeHospitalChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    outcomeHospital = table.Column<int>(type: "INTEGER", nullable: false),
                    outcomeHospitalChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    U071 = table.Column<int>(type: "INTEGER", nullable: false),
                    U071Childrens = table.Column<int>(type: "INTEGER", nullable: false),
                    U072 = table.Column<int>(type: "INTEGER", nullable: false),
                    U072Childrens = table.Column<int>(type: "INTEGER", nullable: false),
                    ORVI = table.Column<int>(type: "INTEGER", nullable: false),
                    ORVIChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    grippe = table.Column<int>(type: "INTEGER", nullable: false),
                    grippeChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    pneumonia = table.Column<int>(type: "INTEGER", nullable: false),
                    pneumoniaChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    OKI = table.Column<int>(type: "INTEGER", nullable: false),
                    OKIChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    meningit = table.Column<int>(type: "INTEGER", nullable: false),
                    meningitChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    hepatit = table.Column<int>(type: "INTEGER", nullable: false),
                    hepatitChildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    HIV = table.Column<int>(type: "INTEGER", nullable: false),
                    HIVCildrens = table.Column<int>(type: "INTEGER", nullable: false),
                    other = table.Column<int>(type: "INTEGER", nullable: false),
                    otherChildrens = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DutyDocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    dutyDoc = table.Column<string>(type: "TEXT", nullable: true),
                    departments = table.Column<string>(type: "TEXT", nullable: true),
                    dutyDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DutyDocs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinalReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    existed = table.Column<int>(type: "INTEGER", nullable: false),
                    existedChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    income = table.Column<int>(type: "INTEGER", nullable: false),
                    incomeChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    outcome = table.Column<int>(type: "INTEGER", nullable: false),
                    outcomeChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    attachedToORIT = table.Column<int>(type: "INTEGER", nullable: false),
                    attachedToORITChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    movedOutDep = table.Column<int>(type: "INTEGER", nullable: false),
                    movedOutDepChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    movedInDep = table.Column<int>(type: "INTEGER", nullable: false),
                    movedInDepChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    died = table.Column<int>(type: "INTEGER", nullable: false),
                    diedChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    present = table.Column<int>(type: "INTEGER", nullable: false),
                    presentChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    oIVL = table.Column<int>(type: "INTEGER", nullable: false),
                    oIVLChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    oNIVL = table.Column<int>(type: "INTEGER", nullable: false),
                    oNIVLChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    oNIVLVPO = table.Column<int>(type: "INTEGER", nullable: false),
                    oNIVLVPOChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    oNIVLMask = table.Column<int>(type: "INTEGER", nullable: false),
                    oNIVLMaskChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    oMask = table.Column<int>(type: "INTEGER", nullable: false),
                    oMaskChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    oBrease = table.Column<int>(type: "INTEGER", nullable: false),
                    oBreaseChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    pregnant = table.Column<int>(type: "INTEGER", nullable: false),
                    pregnantChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    restZone = table.Column<int>(type: "INTEGER", nullable: false),
                    restZoneChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    outRegions = table.Column<int>(type: "INTEGER", nullable: false),
                    outRegionsChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    forein = table.Column<int>(type: "INTEGER", nullable: false),
                    foreinChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    LNRDNR = table.Column<int>(name: "LNR_DNR", type: "INTEGER", nullable: false),
                    LNRDNRChildren = table.Column<int>(name: "LNR_DNRChildren", type: "INTEGER", nullable: false),
                    incomeHospital = table.Column<int>(type: "INTEGER", nullable: false),
                    incomeHospitalChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    outcomeHospital = table.Column<int>(type: "INTEGER", nullable: false),
                    outcomeHospitalChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    U071 = table.Column<int>(type: "INTEGER", nullable: false),
                    U071Children = table.Column<int>(type: "INTEGER", nullable: false),
                    U072 = table.Column<int>(type: "INTEGER", nullable: false),
                    U072Children = table.Column<int>(type: "INTEGER", nullable: false),
                    ORVI = table.Column<int>(type: "INTEGER", nullable: false),
                    ORVIChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    grippe = table.Column<int>(type: "INTEGER", nullable: false),
                    grippeChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    pneumonia = table.Column<int>(type: "INTEGER", nullable: false),
                    pneumoniaChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    OKI = table.Column<int>(type: "INTEGER", nullable: false),
                    OKIChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    meningit = table.Column<int>(type: "INTEGER", nullable: false),
                    meningitChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    hepatit = table.Column<int>(type: "INTEGER", nullable: false),
                    hepatitChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    HIV = table.Column<int>(type: "INTEGER", nullable: false),
                    HIVCildren = table.Column<int>(type: "INTEGER", nullable: false),
                    other = table.Column<int>(type: "INTEGER", nullable: false),
                    otherChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    oSummary = table.Column<int>(type: "INTEGER", nullable: false),
                    oSummaryChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    nozSummary = table.Column<int>(type: "INTEGER", nullable: false),
                    nozSummaryChildren = table.Column<int>(type: "INTEGER", nullable: false),
                    presentNonDaycare = table.Column<int>(type: "INTEGER", nullable: false),
                    presentNonDaycareChildren = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutcomingPatients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", nullable: true),
                    Age = table.Column<string>(type: "TEXT", nullable: true),
                    Shipped = table.Column<string>(type: "TEXT", nullable: true),
                    Diagnos = table.Column<string>(type: "TEXT", nullable: true),
                    SubmitedFrom = table.Column<string>(type: "TEXT", nullable: true),
                    SubmitedTo = table.Column<string>(type: "TEXT", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutcomingPatients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    PersType = table.Column<string>(type: "TEXT", nullable: true),
                    Department = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personels", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepReports");

            migrationBuilder.DropTable(
                name: "DutyDocs");

            migrationBuilder.DropTable(
                name: "FinalReports");

            migrationBuilder.DropTable(
                name: "OutcomingPatients");

            migrationBuilder.DropTable(
                name: "Personels");
        }
    }
}
