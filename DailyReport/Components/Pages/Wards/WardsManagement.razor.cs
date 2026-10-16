using DailyReport.Models;
using DailyReport.Models.WardsModels;
using DailyReport.Services.Reports;
using DailyReport.Services.Wards;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DailyReport.Components.Pages.Wards;

public partial class WardsManagement
{
    [Inject] private WardService WardService { get; set; } = default!;

    [Inject] private ApplicationContext Context { get; set; } = default!;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    [Parameter] public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    public List<Ward> Wards { get; set; } = new();

    public string? Error { get; set; }

    public string NewNumber { get; set; } = string.Empty;

    public int NewCapacity { get; set; }

    public bool IsWardDepartment => Department?.Settings?.ShowOnWards == true;

    protected override void OnParametersSet()
    {
        Department = DepartmentServices.GetById(DepartmentId, Context);
        LoadWards();
    }

    private void LoadWards()
    {
        Wards = Department is null ? new() : WardService.GetWardsByDepartment(DepartmentId);
    }

    private void AddWard()
    {
        if (WardService.Add(DepartmentId, NewNumber, NewCapacity, out var error))
        {
            NewNumber = string.Empty;
            NewCapacity = 0;
            Error = null;
        }
        else
        {
            Error = error;
        }

        LoadWards();
    }

    private void UpdateWard(Ward ward)
    {
        if (!WardService.Update(ward, out var error))
        {
            Error = error;
        }
        else
        {
            Error = null;
        }

        LoadWards();
    }

    private async Task DeleteWard(Ward ward)
    {
        Error = null;

        var number = string.IsNullOrWhiteSpace(ward.Number) ? $"#{ward.Id}" : ward.Number;

        if (WardService.GetPatientsInWard(ward.Department, ward.Number).Count > 0)
        {
            await JsRuntime.InvokeVoidAsync("alert",
                $"Палату «{number}» нельзя удалить: в ней есть пациенты. Сначала переведите или выпишите их.");
            return;
        }

        if (!await JsRuntime.InvokeAsync<bool>("confirm", $"Удалить палату «{number}»?"))
        {
            return;
        }

        if (!WardService.Delete(ward.Id, out var error))
        {
            Error = error;
        }

        LoadWards();
    }
}
