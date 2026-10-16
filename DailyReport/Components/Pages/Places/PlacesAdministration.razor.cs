using DailyReport.Models;
using DailyReport.Services.Wards;
using Microsoft.AspNetCore.Components;

namespace DailyReport.Components.Pages.Places;

public partial class PlacesAdministration
{
    [Inject] private WardService WardService { get; set; } = default!;

    public List<Department> Departments { get; set; } = new();

    protected override void OnInitialized()
    {
        Departments = WardService.GetWardDepartments();
    }
}
