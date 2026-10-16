using DailyReport.Models;
using DailyReport.Models.WardsModels;
using DailyReport.Services.Reports;
using DailyReport.Services.Wards;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DailyReport.Components.Pages.Places;

/// <summary>Состояние палаты на странице «Места» (решение 14.10.2026).</summary>
public enum PlaceStatus
{
    /// <summary>0 пациентов, палата открыта.</summary>
    Free,

    /// <summary>0 &lt; пациентов &lt; мест, палата открыта.</summary>
    Partial,

    /// <summary>пациентов ≥ мест, палата открыта.</summary>
    Full,

    /// <summary><c>CanPut=false</c> — независимо от числа пациентов.</summary>
    Closed
}

/// <summary>
/// Строка-форма палаты (решение пользователя 16.10.2026, по образцу V1
/// <c>Pages/Wards/DepartmentWards.cshtml</c>): в палате столько форм, сколько мест
/// (или пациентов, если их больше). Существующий пациент — редактируемая форма с
/// кнопками «Обновить»/«Удалить» и датой ТОЛЬКО на показ; пустой слот — форма с
/// кнопкой «Сохранить» и редактируемой датой.
/// </summary>
public class PatientRow
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SAge { get; set; } = string.Empty;

    public string Male { get; set; } = "М";

    public string Diagnos { get; set; } = string.Empty;

    public DateTime HospDate { get; set; } = DateTime.Today;

    public bool Rash { get; set; }

    public bool CareRisk { get; set; }

    public bool Untouchable { get; set; }

    /// <summary>Строка уже сохранённого пациента (Id &gt; 0).</summary>
    public bool Exists => Id > 0;

    public string HospDateText => HospDate.ToString("dd.MM.yyyy");
}

/// <summary>Палата вместе со своими пациентами и строками-формами.</summary>
public record WardCard(Ward Ward, List<Patient> Patients)
{
    /// <summary>Видимые строки-формы: сначала сохранённые пациенты, затем пустые слоты,
    /// затем добавленные кнопкой «+ добавить строку» (сверх мест).</summary>
    public List<PatientRow> Rows { get; } = new();

    /// <summary>Свободно мест: max(0, мест − пациентов).</summary>
    public int FreeSlots => Math.Max(0, Ward.Capacity - Patients.Count);

    public PlaceStatus Status =>
        !Ward.CanPut ? PlaceStatus.Closed
        : Patients.Count == 0 ? PlaceStatus.Free
        : Patients.Count >= Ward.Capacity ? PlaceStatus.Full
        : PlaceStatus.Partial;

    public string StatusText => Status switch
    {
        PlaceStatus.Closed => "закрыта",
        PlaceStatus.Full => "заполнена",
        PlaceStatus.Partial => "частично занята",
        _ => "свободна"
    };

    public string StatusClass => Status switch
    {
        PlaceStatus.Closed => "place-card--closed",
        PlaceStatus.Full => "place-card--full",
        PlaceStatus.Partial => "place-card--partial",
        _ => "place-card--free"
    };
}

public partial class PlacesManagement
{
    [Inject] private WardService WardService { get; set; } = default!;

    [Inject] private PatientService PatientService { get; set; } = default!;

    [Inject] private ApplicationContext Context { get; set; } = default!;

    [Inject] private IJSRuntime Js { get; set; } = default!;

    [Parameter] public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    public List<WardCard> Cards { get; set; } = new();

    public string? Error { get; set; }

    public bool IsWardDepartment => Department?.Settings?.ShowOnWards == true;

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
            .Select(ward => BuildCard(ward, PatientsInWard(patients, ward.Number)))
            .ToList();
    }

    /// <summary>Пациенты палаты: связь по паре (Department, WardNumber), сравнение в памяти.</summary>
    private static List<Patient> PatientsInWard(List<Patient> patients, string wardNumber)
    {
        var number = (wardNumber ?? string.Empty).Trim();

        return patients
            .Where(p => string.Equals((p.WardNumber ?? string.Empty).Trim(), number, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>Строки палаты: max(мест, пациентов) — сохранённые сверху, ниже пустые слоты.</summary>
    private static WardCard BuildCard(Ward ward, List<Patient> patients)
    {
        var card = new WardCard(ward, patients);
        var count = Math.Max(ward.Capacity, patients.Count);

        for (var i = 0; i < count; i++)
        {
            card.Rows.Add(i < patients.Count ? FromPatient(patients[i]) : NewRow());
        }

        return card;
    }

    private static PatientRow FromPatient(Patient patient) => new()
    {
        Id = patient.Id,
        Name = patient.Name ?? string.Empty,
        SAge = patient.sAge ?? string.Empty,
        Male = string.IsNullOrWhiteSpace(patient.Male) ? "М" : patient.Male,
        Diagnos = patient.Diagnos ?? string.Empty,
        HospDate = patient.HospitalisationDate,
        Rash = patient.HasRash,
        CareRisk = patient.HasCareRisk,
        Untouchable = patient.IsUntochable
    };

    private static PatientRow NewRow() => new()
    {
        Male = "М",
        HospDate = DateTime.Today
    };

    /// <summary>Кнопка «+ добавить строку» в шапке: добавляет ПУСТУЮ форму сверх числа мест.</summary>
    public void AddRow(WardCard card)
    {
        Error = null;
        card.Rows.Add(NewRow());
    }

    /// <summary>«Сохранить» (Id=0 — новый пациент) или «Обновить» (Id&gt;0 — правка).</summary>
    public void SaveRow(WardCard card, PatientRow row)
    {
        if (Department is null)
        {
            Error = "Отделение не найдено";
            return;
        }

        var patient = new Patient
        {
            Id = row.Id,
            Department = DepartmentId,
            WardNumber = card.Ward.Number,
            Name = row.Name,
            Male = row.Male,
            sAge = row.SAge,
            Diagnos = row.Diagnos,
            HospitalisationDate = row.HospDate,
            HasRash = row.Rash,
            HasCareRisk = row.CareRisk,
            IsUntochable = row.Untouchable
        };

        var ok = row.Id == 0
            ? PatientService.Add(patient, out var error)
            : PatientService.Update(patient, out error);

        if (!ok)
        {
            Error = error;
            return;
        }

        Error = null;
        LoadCards();
    }

    public async Task DeleteRow(PatientRow row)
    {
        if (!row.Exists)
        {
            return;
        }

        var confirmed = await Js.InvokeAsync<bool>("confirm", $"Удалить пациента «{row.Name}» из палаты?");
        if (!confirmed)
        {
            return;
        }

        Error = PatientService.Delete(row.Id, out var error) ? null : error;
        LoadCards();
    }

    private void ToggleClosed(Ward ward)
    {
        Error = WardService.SetCanPut(ward.Id, !ward.CanPut, out var error) ? null : error;
        LoadCards();
    }

    private IJSObjectReference? dragger;
    private DotNetObjectReference<PlacesManagement>? selfRef;

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
    /// Вызывается из <c>wwwroot/js/placeDrag.js</c> при дропе пациента на карточку палаты.
    /// Перенос только внутри отделения (меняется <c>WardNumber</c>). Дроп в закрытую или
    /// переполненную палату разрешён, но требует подтверждения (решение 14.10.2026).
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
            .Select(card => (Card: card, Row: card.Rows.FirstOrDefault(r => r.Id == patientId)))
            .FirstOrDefault(x => x.Row is not null);

        if (source.Row is null)
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
                "confirm", $"Палата {target.Ward.Number} {reason} — перенести пациента «{source.Row.Name}»?");

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
