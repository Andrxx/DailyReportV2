using DailyReport.Models;
using DailyReport.Models.PersonelFolder;
using DailyReport.Services.Reports;
using DailyReport.Services.Reporting;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using FireReportEntity = DailyReport.Models.Reports.FireReport;

namespace DailyReport.Components.Pages.Reports
{
    /// <summary>
    /// Итоговая пожарная сводка по больнице (разрез 2) за текущие отчётные сутки пожарной
    /// сводки (граница 08:00). Страница только для чтения: БД не изменяется.
    /// Долгохранимые подразделения (охрана, общежитие) показывают последнюю сохранённую
    /// строку, остальные — строку за текущие сутки.
    /// </summary>
    public partial class FireSumReport
    {
        [Inject] private FireReportService FireReportService { get; set; } = null!;
        [Inject] private DutyNurseService DutyNurseService { get; set; } = null!;
        [Inject] private FireReportingCalendar FireCalendar { get; set; } = null!;
        [Inject] private ApplicationContext Context { get; set; } = null!;
        [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

        /// <summary>Отчётные сутки пожарной сводки открытой таблицы.</summary>
        public DateOnly ReportDate { get; set; }

        /// <summary>Строки сводки: по одной на подразделение, в порядке ShowOrder.</summary>
        public List<FireRow> Rows { get; set; } = new();

        /// <summary>
        /// Дежурные медсёстры и контакты технических служб текущих суток, сгруппированные по
        /// подразделениям. Выводятся ВСЕ подразделения пожарной сводки в порядке ShowOrder,
        /// даже если контактов нет: пустое подразделение видно по подписи «Не назначены».
        /// </summary>
        public List<NurseGroup> NurseGroups { get; set; } = new();

        /// <summary>Ячейка сводки: «—», если сводка подразделения не подана.</summary>
        public static string Cell(int? value) => value?.ToString() ?? "—";

        protected override void OnInitialized()
        {
            ReportDate = FireCalendar.Today;

            Rows = DepartmentServices.GetFireReportDepartments(Context)
                .Select(dep => new FireRow(dep, PickReport(dep)))
                .ToList();

            // Дежурные сёстры — по подразделениям в порядке ShowOrder. Показываем все
            // подразделения (как в первой версии). К дежурным сёстрам отделения добавляются
            // контакты технических служб из конфигурации (AddedNurses) с тем же department.
            var nurses = DutyNurseService.GetForDay();
            var added = DutyNurseService.GetConfiguredNurses();
            NurseGroups = Rows
                .Select(row => new NurseGroup(
                    row.Department.Name ?? string.Empty,
                    nurses.Where(n => n.department == row.Department.Id)
                        .Select(n => new NurseEntry(n.name ?? string.Empty, n.Phone))
                        .Concat(added.Where(a => a.Department == row.Department.Id)
                            .Select(a => new NurseEntry(a.Name, a.Phone)))
                        .ToList(),
                    row.Department.Settings?.Type == DepartmentType.Utility))
                .ToList();
        }

        /// <summary>
        /// Сводка подразделения для строки: долгохранимым берётся последняя сохранённая
        /// строка, остальным — строка за текущие отчётные сутки.
        /// </summary>
        private FireReportEntity? PickReport(Department dep)
        {
            return dep.Settings?.Type.IsLongStoredFireReport() == true
                ? FireReportService.GetLastByNumber(dep.Id)
                : FireReportService.GetByNumber(dep.Id, ReportDate);
        }

        /// <summary>Печать только списка контактов (дежурные сёстры и тех.службы).</summary>
        private async Task PrintPersonnel()
        {
            await JsRuntime.InvokeVoidAsync("firePrint", "personnel");
        }

        /// <summary>Печать только таблицы итоговой сводки.</summary>
        private async Task PrintTable()
        {
            await JsRuntime.InvokeVoidAsync("firePrint", "table");
        }

        /// <summary>Строка пожарной сводки. Числа null, пока сводка подразделения не подана.</summary>
        public sealed record FireRow(Department Department, FireReportEntity? Report)
        {
            public int? Adult => Report?.Adult;

            public int? Children => Report?.Children;

            public int? Care => Report?.Care;

            /// <summary>Нетранспортабельные взрослые — подмножество <see cref="Adult"/>, только показ.</summary>
            public int? NonTransportableAdults => Report?.NonTransportableAdults;

            /// <summary>Нетранспортабельные дети — подмножество <see cref="Children"/>, только показ.</summary>
            public int? NonTransportableChildren => Report?.NonTransportableChildren;

            public int? Doctors => Report?.Doctors;

            public int? Nurses => Report?.Nurses;

            public int? Sanitarians => Report?.Sanitarians;

            /// <summary>
            /// Технический персонал (охрана, проживающие в общежитии) — поле `Personel`.
            /// Уже входит в «Всего человек», поэтому в итоги отдельно не добавляется.
            /// </summary>
            public int? Personel => Report?.Personel;

            /// <summary>«Всего взр» — как в первой версии: взрослые плюс лица по уходу.</summary>
            public int? TotalAdults => Report is null ? null : Report.Adult + Report.Care;

            /// <summary>«Всего дет» — как в первой версии: дети без ухода.</summary>
            public int? TotalChildren => Report?.Children;

            /// <summary>«Всего человек»: все категории вместе, включая персонал.</summary>
            public int? TotalPeople => Report is null ? null
                : Report.Adult + Report.Children + Report.Care + Report.Doctors + Report.Nurses
                  + Report.Sanitarians + Report.Personel;

            /// <summary>Время сохранения сводки.</summary>
            public string? Time => Report is null ? null : Report.Date.ToString("HH:mm dd.MM.yy");
        }

        /// <summary>
        /// Дежурные сёстры и контакты одного подразделения для списка сводки.
        /// <paramref name="SkipInPrint"/> — техническое/хозяйственное подразделение (общежитие):
        /// на экране блок остаётся, на бумаге не печатается.
        /// </summary>
        public sealed record NurseGroup(string DepartmentName, List<NurseEntry> Entries, bool SkipInPrint);

        /// <summary>Один контакт в списке сводки: ФИО (или название службы) и телефон.</summary>
        public sealed record NurseEntry(string Name, string? Phone);
    }
}