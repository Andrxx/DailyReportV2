using DailyReport.Models;
using DailyReport.Models.WardsModels;
using DailyReport.Services.Reports;
using DailyReport.Services.Wards;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DailyReport.Components.Pages.Places;

public enum SamsonPlaceStatus
{
    /// <summary>0 пациентов, палата открыта.</summary>
    Free,

    /// <summary>0 &lt; пациентов &lt; мест, палата открыта.</summary>
    Partial,

    /// <summary>пациентов = мест, палата открыта — это норма, не ошибка.</summary>
    Full,

    /// <summary>пациентов &gt; мест — единственное «красное» состояние.</summary>
    Overflow,

    /// <summary><c>CanPut=false</c> — независимо от числа пациентов.</summary>
    Closed
}

public enum PatientFlag
{
    Rash,
    CareRisk,
    Untouchable
}

/// <summary>Палата витрины «Места(Самсон)»: пациенты и вычисляемые состояние/заполненность.</summary>
public class SamsonWardCard
{
    public SamsonWardCard(Ward ward, List<Patient> patients)
    {
        Ward = ward;
        Patients = patients;
    }

    public Ward Ward { get; }

    public List<Patient> Patients { get; }

    /// <summary>Строк в таблице: max(мест, пациентов).</summary>
    public int RowCount => Math.Max(Ward.Capacity, Patients.Count);

    /// <summary>Свободно мест: max(0, мест − пациентов).</summary>
    public int FreeSlots => Math.Max(0, Ward.Capacity - Patients.Count);

    public Patient? PatientAt(int index) =>
        index >= 0 && index < Patients.Count ? Patients[index] : null;

    public int Percent
    {
        get
        {
            if (Ward.Capacity <= 0)
            {
                return Patients.Count > 0 ? 100 : 0;
            }

            return Math.Min(100, (int)Math.Round(Patients.Count * 100.0 / Ward.Capacity));
        }
    }

    public SamsonPlaceStatus Status =>
        !Ward.CanPut ? SamsonPlaceStatus.Closed
        : Patients.Count > Ward.Capacity ? SamsonPlaceStatus.Overflow
        : Patients.Count == 0 ? SamsonPlaceStatus.Free
        : Patients.Count >= Ward.Capacity ? SamsonPlaceStatus.Full
        : SamsonPlaceStatus.Partial;

    public string StatusText => Status switch
    {
        SamsonPlaceStatus.Closed => "закрыта",
        SamsonPlaceStatus.Overflow => "переполнена",
        SamsonPlaceStatus.Full => "заполнена",
        SamsonPlaceStatus.Partial => "частично занята",
        _ => "свободна"
    };

    public string StatusClass => Status switch
    {
        SamsonPlaceStatus.Closed => "samson-card--closed",
        SamsonPlaceStatus.Overflow => "samson-card--overflow",
        SamsonPlaceStatus.Full => "samson-card--full",
        SamsonPlaceStatus.Partial => "samson-card--partial",
        _ => "samson-card--free"
    };
}

/// <summary>
/// Витрина палат «Места(Самсон)» (формализовано и реализовано 20.10.2026).
/// Паспортные данные пациента — только чтение; флаги, статус палаты и перетаскивание
/// доступны. Добавление/удаление пациентов не используется — методы сервисов остаются
/// нетронутыми (см. AGENTS.md).
/// </summary>
public partial class SamsonPlacesBoard : IAsyncDisposable
{
    [Inject] private WardService WardService { get; set; } = default!;

    [Inject] private PatientService PatientService { get; set; } = default!;

    [Inject] private ApplicationContext Context { get; set; } = default!;

    [Inject] private IJSRuntime Js { get; set; } = default!;

    [Parameter] public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    public List<SamsonWardCard> Cards { get; set; } = new();

    public string? Error { get; set; }

    public bool IsWardDepartment => Department?.Settings?.ShowOnWards == true;

    public int BedsTotal => Cards.Sum(card => card.Ward.Capacity);

    public int OccupiedTotal => Cards.Sum(card => card.Patients.Count);

    public int FreeTotal => Cards.Sum(card => card.FreeSlots);

    public int ClosedWards => Cards.Count(card => !card.Ward.CanPut);

    protected override void OnParametersSet()
    {
        Department = DepartmentServices.GetById(DepartmentId, Context);
        Error = null;
        LoadCards();
    }

    private void LoadCards()
    {
        if (Department is null)
        {
            Cards = new();
            return;
        }

        var patients = PatientService.GetPatientsByDepartment(DepartmentId);

        Cards = WardService.GetWardsByDepartment(DepartmentId)
            .Select(ward => new SamsonWardCard(ward, PatientsInWard(patients, ward.Number)))
            .ToList();
    }

    private static List<Patient> PatientsInWard(List<Patient> patients, string wardNumber)
    {
        var number = (wardNumber ?? string.Empty).Trim();

        return patients
            .Where(p => string.Equals((p.WardNumber ?? string.Empty).Trim(), number, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public void ToggleClosed(SamsonWardCard card)
    {
        Error = WardService.SetCanPut(card.Ward.Id, !card.Ward.CanPut, out var error) ? null : error;
        LoadCards();
    }

    /// <summary>
    /// Флаг пациента. Паспортные данные не меняются: запись читается заново и
    /// переписывается штатным <c>PatientService.Update</c> с изменённым флагом.
    /// </summary>
    public void SetFlag(int patientId, PatientFlag flag, ChangeEventArgs args)
    {
        var value = args.Value is bool b && b;

        var patient = PatientService.GetPatient(patientId);
        if (patient is null)
        {
            Error = "Пациент не найден.";
            LoadCards();
            return;
        }

        switch (flag)
        {
            case PatientFlag.Rash:
                patient.HasRash = value;
                break;
            case PatientFlag.CareRisk:
                patient.HasCareRisk = value;
                break;
            case PatientFlag.Untouchable:
                patient.IsUntochable = value;
                break;
        }

        Error = PatientService.Update(patient, out var error) ? null : error;
        LoadCards();
    }

    private IJSObjectReference? dragger;
    private DotNetObjectReference<SamsonPlacesBoard>? selfRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        selfRef = DotNetObjectReference.Create(this);
        dragger = await Js.InvokeAsync<IJSObjectReference>("import", "./js/placeDrag.js");
        await dragger.InvokeVoidAsync("init", selfRef);
    }

    /// <summary>
    /// Дроп из <c>wwwroot/js/placeDrag.js</c>: перенос только внутри отделения
    /// (меняется <c>WardNumber</c>). В закрытую или переполненную палату — с подтверждением.
    /// </summary>
    [JSInvokable]
    public async Task DropPatient(int patientId, string wardNumber)
    {
        if (Department is null)
        {
            return;
        }

        wardNumber = (wardNumber ?? string.Empty).Trim();

        var source = Cards
            .Select(card => (Card: card, Patient: card.Patients.FirstOrDefault(p => p.Id == patientId)))
            .FirstOrDefault(x => x.Patient is not null);

        if (source.Patient is null)
        {
            return;
        }

        var sourceNumber = (source.Card.Ward.Number ?? string.Empty).Trim();
        if (string.Equals(sourceNumber, wardNumber, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var target = Cards.FirstOrDefault(card =>
            string.Equals((card.Ward.Number ?? string.Empty).Trim(), wardNumber, StringComparison.OrdinalIgnoreCase));

        if (target is null)
        {
            Error = $"Палата «{wardNumber}» в этом отделении не найдена.";
            await InvokeAsync(StateHasChanged);
            return;
        }

        if (!target.Ward.CanPut || target.Patients.Count >= target.Ward.Capacity)
        {
            var reason = !target.Ward.CanPut ? "закрыта" : "переполнена";
            var confirmed = await Js.InvokeAsync<bool>(
                "confirm", $"Палата {target.Ward.Number} {reason} — перенести пациента «{source.Patient.Name}»?");

            if (!confirmed)
            {
                return;
            }
        }

        Error = PatientService.MoveToWard(patientId, wardNumber, out var error) ? null : error;
        LoadCards();
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        if (dragger is not null)
        {
            try
            {
                await dragger.InvokeVoidAsync("release");
                await dragger.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // контур уже закрыт (перезагрузка/уход со страницы) — чистить нечего
            }
        }

        selfRef?.Dispose();
    }
}
