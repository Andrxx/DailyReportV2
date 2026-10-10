using DailyReport.Models.PersonelFolder;
using DailyReport.Services.Personnel;
using Microsoft.AspNetCore.Components;

namespace DailyReport.Components.Pages.Admin;

public partial class PersonnelManagement
{
    [Inject] private PersonelService PersonelService { get; set; } = default!;

    [Inject] private IConfiguration AppConfig { get; set; } = default!;

    public List<Personel> Personels { get; set; } = new();

    public List<string> PersTypes { get; set; } = new();

    public List<string> Departments { get; set; } = new();

    public Personel NewPersonel { get; set; } = new();

    /// <summary>Выбранный в селекте пресет должности; пусто, если должность своя.</summary>
    public string PersTypeSelect { get; set; } = string.Empty;

    /// <summary>Выбранное в селекте отделение; пусто, если подразделение своё.</summary>
    public string DepartmentSelect { get; set; } = string.Empty;

    public string? Error { get; set; }

    protected override void OnInitialized()
    {
        Load();
    }

    private void Load()
    {
        Personels = PersonelService.GetAll();
        Departments = PersonelService.GetDepartmentNames();
        PersTypes = ReadPersTypes();
    }

    /// <summary>
    /// Должности-пресеты из секции <c>Personnel</c> конфигурации. Читается по индексам
    /// (Personnel:PersTypes:0, …): секции нет или она пуста — возвращается пустой список,
    /// поле остаётся текстовым без подсказок.
    /// </summary>
    private List<string> ReadPersTypes()
    {
        var list = new List<string>();

        for (var i = 0; ; i++)
        {
            var item = AppConfig[$"Personnel:PersTypes:{i}"];

            if (item is null)
            {
                break;
            }

            if (!string.IsNullOrWhiteSpace(item))
            {
                list.Add(item.Trim());
            }
        }

        return list;
    }

    private void AddPersonel()
    {
        if (PersonelService.Add(NewPersonel, out var error))
        {
            NewPersonel = new Personel();
            PersTypeSelect = string.Empty;
            DepartmentSelect = string.Empty;
            Error = null;
        }
        else
        {
            Error = error;
        }

        Load();
    }

    private void OnPersTypeSelect(ChangeEventArgs e)
    {
        var value = e.Value?.ToString() ?? string.Empty;
        NewPersonel.PersType = value;
        PersTypeSelect = value;
    }

    private void OnPersTypeInput(ChangeEventArgs e)
    {
        NewPersonel.PersType = e.Value?.ToString() ?? string.Empty;
        PersTypeSelect = PersTypes.Contains(NewPersonel.PersType) ? NewPersonel.PersType : string.Empty;
    }

    private void OnDepartmentSelect(ChangeEventArgs e)
    {
        var value = e.Value?.ToString() ?? string.Empty;
        NewPersonel.Department = value;
        DepartmentSelect = value;
    }

    private void OnDepartmentInput(ChangeEventArgs e)
    {
        NewPersonel.Department = e.Value?.ToString() ?? string.Empty;
        DepartmentSelect = Departments.Contains(NewPersonel.Department) ? NewPersonel.Department : string.Empty;
    }

    private void UpdatePersonel(Personel person)
    {
        if (PersonelService.Update(person, out var error))
        {
            Error = null;
        }
        else
        {
            Error = error;
        }

        Load();
    }

    private void DeletePersonel(int id)
    {
        PersonelService.Delete(id);
        Error = null;
        Load();
    }
}
