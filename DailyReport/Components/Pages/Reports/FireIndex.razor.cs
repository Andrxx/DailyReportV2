using DailyReport.Models;
using DailyReport.Services.Reports;
using Microsoft.AspNetCore.Components;

namespace DailyReport.Components.Pages.Reports
{
    /// <summary>
    /// Список подразделений пожарной сводки (разрез 2). В отличие от главной страницы,
    /// сюда входят и непрофильные подразделения: лаборатория, охрана, общежитие.
    /// </summary>
    public partial class FireIndex
    {
        [Inject] public ApplicationContext Context { get; set; } = default!;

        public List<Department> Departments { get; set; } = new();

        protected override void OnInitialized()
        {
            Departments = DepartmentServices.GetFireReportDepartments(Context);
        }
    }
}