using DailyReport.Models;
using DailyReport.Services.Wards;
using Microsoft.AspNetCore.Components;

namespace DailyReport.Components.Pages.Places;

/// <summary>
/// Индекс «Места(Самсон)»: список отделений, участвующих в размещении
/// (<c>ShowOnWards</c>). Последняя строка — «Вся больница», ссылка на общую
/// сводку палат (по аналогии с общими сводками, 22.10.2026).
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
