using DailyReport.Models;
using DailyReport.Services.Reports;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace DailyReport.Components.Pages.Admin;

public partial class DepartmentsManagement
{
    [Inject] private ApplicationContext Context { get; set; } = default!;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    public List<Department> Departments { get; set; } = new();

    public Department NewDepartment { get; set; } = new();

    public bool SettingsTableExists { get; set; }

    protected override void OnInitialized()
    {
        LoadDepartments();
    }

    private void LoadDepartments()
    {
        SettingsTableExists = Context.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sqlite_master WHERE type='table' AND name='DepartmentSettings'")
            .First() > 0;

        IQueryable<Department> query = Context.Departments.AsNoTracking();
        if (SettingsTableExists) query = query.Include(d => d.Settings);
        Departments = query.OrderBy(d => d.ShowOrder).ToList();
    }

    /// <summary>
    /// Перед любой записью сбрасывает изменения трекера. Страница работает с КОПИЯМИ
    /// (список загружен AsNoTracking), а в том же контуре от других страниц остаются
    /// отслеживаемые экземпляры Department/DepartmentSettings — отделение через
    /// DepartmentServices.GetById на /wards, /depreport и /firereport, Department
    /// и его настройки через Add/Update. Без сброса Update/Remove падает с
    /// «another instance with the same key value for {'DepartmentId'} is already tracked».
    /// Незаписанных изменений на странице нет: каждая операция сразу заканчивается SaveChanges.
    /// </summary>
    private void ResetTracker() => Context.ChangeTracker.Clear();

    private void AddDepartment()
    {
        ResetTracker();
        DepartmentServices.AddDepartment(NewDepartment, Context);
        NewDepartment = new Department();
        LoadDepartments();
    }

    private void UpdateDepartment(Department dep)
    {
        ResetTracker();
        DepartmentServices.UpdateDepartment(dep, Context);
        LoadDepartments();
    }

    private async Task DeleteDepartment(Department dep)
    {
        var name = string.IsNullOrWhiteSpace(dep.Name) ? $"отделение #{dep.Id}" : dep.Name;
        if (!await JsRuntime.InvokeAsync<bool>("confirm",
            $"Удалить отделение «{name}»?\nОтделение и его настройки будут удалены безвозвратно; сводки за прошлые дни останутся в базе."))
        {
            return;
        }

        ResetTracker();
        DepartmentServices.DeleteDepartment(dep.Id, Context);
        LoadDepartments();
    }

    private void SaveType(Department dep)
    {
        if (dep?.Settings is null || !SettingsTableExists) return;
        SaveSettingsRow(dep.Settings);
    }

    private void ToggleSettings(Department dep, string property, bool value)
    {
        if (dep?.Settings is null || !SettingsTableExists) return;
        switch (property)
        {
            case nameof(DepartmentSettings.ShowOnMainPage): dep.Settings.ShowOnMainPage = value; break;
            case nameof(DepartmentSettings.ShowOnFinalReport): dep.Settings.ShowOnFinalReport = value; break;
            case nameof(DepartmentSettings.CountSpotsInFinalReport): dep.Settings.CountSpotsInFinalReport = value; break;
            case nameof(DepartmentSettings.ShowOnFireReport): dep.Settings.ShowOnFireReport = value; break;
            case nameof(DepartmentSettings.ShowOnWards): dep.Settings.ShowOnWards = value; break;
            case nameof(DepartmentSettings.SimpleReportForm): dep.Settings.SimpleReportForm = value; break;
        }
        SaveSettingsRow(dep.Settings);
    }

    private void SaveSettingsRow(DepartmentSettings settings)
    {
        ResetTracker();
        settings.Department = null; // не захватывать граф отделения при Update
        Context.DepartmentSettings.Update(settings);
        Context.SaveChanges();
    }
}