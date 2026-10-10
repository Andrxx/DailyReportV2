using DailyReport.Models;
using DailyReport.Services.Reporting;
using DailyReport.Services.Reports;
using Microsoft.AspNetCore.Components;

namespace DailyReport.Components.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] public ApplicationContext Context { get; set; } = default!;
        [Inject] public ReportingCalendar Calendar { get; set; } = default!;

        public List<Department> Departments { get; set; } = new();

        /// <summary>
        /// Отчётные сутки, которые показывают на планёрке: предыдущие от текущих.
        /// Берётся ReportingCalendar, а не календарная вчерашняя дата: сутки
        /// переключаются в 07:30, до этого момента «вчера» ещё не наступила.
        /// </summary>
        public DateOnly PlanDay => Calendar.Today.AddDays(-1);

        /// <summary>Адрес итоговой сводки за сутки планёрки.</summary>
        public string PlanReportUrl => $"/finalreport?date={PlanDay:yyyy-MM-dd}";

        /// <summary>Подсказка со ссылкой: какое число откроется.</summary>
        public string PlanDayTitle => $"Сводка за {PlanDay:dd.MM.yyyy}";

        protected override void OnInitialized()
        {
            Departments = DepartmentServices.GetMainPageDepartments(Context);
        }
    }
}
