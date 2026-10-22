using DailyReport.Models;
using DailyReport.Services.Wards;
using Microsoft.AspNetCore.Components;

namespace DailyReport.Components.Pages.Places;

/// <summary>
/// Индекс «Места(Самсон)»: голый список отделений, участвующих в размещении
/// (<c>ShowOnWards</c>), без сводок и KPI (решение 20.10.2026).
/// </summary>
public partial class SamsonPlacesAdministration
{
    [Inject] private WardService WardService { get; set; } = default!;

    public List<Department> Departments { get; set; } = new();

    protected override void OnInitialized()
    {
        Departments = WardService.GetWardDepartments();
    }
}
