using DailyReport.Services.Places;
using DailyReport.Services.Reporting;
using DailyReport.Services.Wards;
using Microsoft.AspNetCore.Components;

namespace DailyReport.Components.Pages.Places;

public partial class SamsonDashboard
{
    [Inject] private WardService WardService { get; set; } = default!;

    [Inject] private ReportingCalendar Calendar { get; set; } = default!;

    public WardBoardData Board { get; set; } = new();

    public DateOnly ReportDate { get; set; }

    protected override void OnInitialized()
    {
        ReportDate = Calendar.Today;
        Board = WardService.GetWardBoard();
    }
}
