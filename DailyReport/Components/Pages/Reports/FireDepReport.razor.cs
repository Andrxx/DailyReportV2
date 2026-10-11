using DailyReport.Models;
using DailyReport.Models.Reports;
using DailyReport.Services.Reports;
using DailyReport.Services.Reporting;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using FireReportEntity = DailyReport.Models.Reports.FireReport;

namespace DailyReport.Components.Pages.Reports
{
    /// <summary>
    /// Форма пожарной сводки подразделения (разрез 2). Сутки у пожарной сводки свои
    /// (граница 08:00), прошлые даты не открываются — форма показывает только
    /// текущие отчётные сутки.
    /// Долгохранимым подразделениям (охрана, общежитие) в форме показывается последняя
    /// сохранённая строка: значение задаётся один раз и берётся в сводке до его изменения.
    /// </summary>
    public partial class FireDepReport
    {
        [Inject] private FireReportService FireReportService { get; set; } = null!;
        [Inject] private FireReportingCalendar FireCalendar { get; set; } = null!;
        [Inject] private ApplicationContext Context { get; set; } = null!;
        [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

        [Parameter] public int DepartmentId { get; set; }

        public Department? Department { get; set; }

        public FireReportEntity? Report { get; set; }

        /// <summary>Отчётные сутки пожарной сводки открытой строки.</summary>
        public DateOnly ReportDate { get; set; }

        /// <summary>Долгохранимое подразделение — охрана и общежитие.</summary>
        public bool IsLongStored => Department?.Settings?.Type.IsLongStoredFireReport() == true;

        /// <summary>Подразделение участвует в пожарной сводке (разрез 2).</summary>
        public bool IsFireDepartment => Department?.Settings?.ShowOnFireReport == true;

        /// <summary>Отметка о времени последнего сохранения.</summary>
        public string SavedTime { get; private set; } = string.Empty;

        /// <summary>Отклик на успешное сохранение: полоса по кнопке, как в сводке отделения.</summary>
        private bool SaveSweep { get; set; }

        protected override void OnInitialized()
        {
            Department = DepartmentServices.GetById(DepartmentId, Context);
            if (Department is null) return;

            ReportDate = FireCalendar.Today;
            LoadReport();
        }

        /// <summary>
        /// Загрузка сводки за текущие отчётные сутки. Долгохранимым подразделениям
        /// при отсутствии строки за сутки подставляется последняя сохранённая —
        /// их значение живёт, пока его не изменят.
        /// </summary>
        private void LoadReport()
        {
            Report = FireReportService.GetByNumber(DepartmentId, ReportDate);

            if (Report is null && IsLongStored)
            {
                var last = FireReportService.GetLastByNumberNoTracking(DepartmentId);

                Report = new FireReportEntity
                {
                    DepNumber = DepartmentId,
                    Adult = last?.Adult ?? 0,
                    Children = last?.Children ?? 0,
                    Care = last?.Care ?? 0,
                    Doctors = last?.Doctors ?? 0,
                    Nurses = last?.Nurses ?? 0,
                    Sanitarians = last?.Sanitarians ?? 0,
                    Personel = last?.Personel ?? 0
                };
            }

            if (Report is null)
            {
                Report = new FireReportEntity { DepNumber = DepartmentId };
            }

            Report.ReportDate = ReportDate;
            SavedTime = Report.Id == 0 ? string.Empty : Report.Date.ToString("HH:mm dd.MM.yyyy");
        }

        /// <summary>Сохранение пожарной сводки. Дата суток не пересчитывается: форма её задаёт.</summary>
        private async Task Save()
        {
            if (Report is null) return;

            Report.DepNumber = DepartmentId;
            Report.ReportDate = ReportDate;
            Report.Date = FireCalendar.LocalNow;
            await Task.Run(() => FireReportService.AddOrUpdate(Report));

            // Отклик только после успешной записи. StateHasChanged обязателен: Blazor
            // не перерисовывает компонент между await внутри обработчика.
            SavedTime = Report.Date.ToString("HH:mm dd.MM.yyyy");
            SaveSweep = true;
            await InvokeAsync(StateHasChanged);
            await Task.Delay(1000);
            SaveSweep = false;
        }

        private async Task Print()
        {
            await JsRuntime.InvokeVoidAsync("window.print");
        }
    }
}