using System.Globalization;
using DailyReport.Models;
using DailyReport.Models.PersonelFolder;
using DailyReport.Models.Reports;
using DailyReport.Services.Personnel;
using DailyReport.Services.Reports;
using DailyReport.Services.Reporting;
using DailyReport.Services.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using DepReportEntity = DailyReport.Models.Reports.DepReport;

namespace DailyReport.Components.Pages.Reports
{
    /// <summary>
    /// Контракт (view-model) страницы заполнения сводки отделения.
    /// Содержит состояние компонента и логику загрузки данных из БД и конфигурации.
    /// </summary>
    public partial class DepReport
    {
        [Inject] private DepReportService DepReportService { get; set; } = null!;
        [Inject] private PersonelService PersonelService { get; set; } = null!;
        [Inject] private ReportingCalendar Calendar { get; set; } = null!;
        [Inject] private IAccessControl AccessControl { get; set; } = null!;
        [Inject] private IConfiguration AppConfig { get; set; } = null!;
        [Inject] private ApplicationContext Context { get; set; } = null!;
        [Inject] private NavigationManager Navigation { get; set; } = null!;
        [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

        [Parameter] public int DepartmentId { get; set; }

        // --- отделение и его роль ---
        public Department? Department { get; set; }
        public DepartmentType DepartmentType => Department?.Settings?.Type ?? DepartmentType.General;

        /// <summary>Отделение заполняет сводку по пациентам (разрез 1)</summary>
        public bool IsPatientDepartment => DepartmentType.CollectsPatientReport();

        /// <summary>Упрощённая форма КДО/платных услуг — только «Обратилось»</summary>
        public bool IsSimpleForm => DepartmentType.IsSimplePatientForm();

        /// <summary>ОРИТ: строки движения не заполняются</summary>
        private bool IsOrit => DepartmentType.IsOrit();

        // --- данные отчёта ---
        public DepReportEntity? Report { get; set; }

        /// <summary>Отчётные сутки открытой сводки, то есть чистая дата.</summary>
        public DateOnly ReportDate { get; set; }
        public string DutyNurse { get; set; } = string.Empty;

        /// <summary>
        /// Медсёстры для селектора дежурной м/с. Двойная сортировка: сначала медсёстры
        /// текущего отделения (по алфавиту), затем все остальные (по алфавиту).
        /// </summary>
        public List<Personel> Nurses { get; set; } = new();

        // --- конфигурируемые строки (названия опциональных показателей) ---
        public string NozologyName { get; set; } = string.Empty;
        public string OxygenName { get; set; } = string.Empty;
        public string SocialName { get; set; } = string.Empty;
        public string CareName { get; set; } = string.Empty;

        // --- видимость опциональных строк ---
        public bool NozologyVisible { get; set; }
        public bool OxygenVisible { get; set; }
        public bool SocialVisible { get; set; }
        public bool CareVisible { get; set; }

        // --- автоподсчёт "Состоит" ---
        // Формула перенесена из первой версии (CountSum в WardScripts/site.js V1):
        // состояло − выбыло + поступило − переведено из отделения + переведено в отделение − умерло.
        // Поле «Состоит» не редактируется, значение вычисляется и сохраняется в Save.
        public int PresentComputed =>
            Report is null
                ? 0
                : Report.existed - Report.outcome + Report.income - Report.movedOutDep + Report.movedInDep - Report.died;

        public int PresentChildrensComputed =>
            Report is null
                ? 0
                : Report.existedChildren - Report.outcomeChildrens + Report.incomeChildrens
                  - Report.movedOutDepChildrens + Report.movedInDepChildrens - Report.diedChildrens;

        // --- внутренние параметры ---
        /// <summary>
        /// Самые ранние отчётные сутки, доступные для выбора. Бизнес-ограничения на
        /// глубину перехода нет: править можно любую прошлую дату. Дно нужно, чтобы
        /// календарь не позволял уйти в несуществующие даты.
        /// </summary>
        private static DateOnly MinReportDate => DateOnly.FromDateTime(new DateTime(2000, 1, 1));

        /// <summary>Открыты не текущие отчётные сутки.</summary>
        public bool IsPastDay => ReportDate != Calendar.Today;

        /// <summary>Отчётные сутки, выбранные в календаре. null — дата не выбрана.</summary>
        public DateOnly? PickDate { get; set; }

        /// <summary>
        /// Верхняя граница календаря: текущие отчётные сутки. Будущее выбрать нельзя.
        /// </summary>
        public string MaxPickDate => Calendar.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        /// <summary>Нижняя граница календаря.</summary>
        public string MinPickDate => MinReportDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        /// <summary>Пароль, введённый в форму перехода к прошлым датам.</summary>
        public string UnlockPassword { get; set; } = string.Empty;

        /// <summary>Текст последней ошибки перехода.</summary>
        public string? UnlockError { get; set; }

        /// <summary>Причина, по которой прошлые даты недоступны, или null если доступ возможен.</summary>
        public string? AccessUnavailableReason => AccessControl.GetUnavailableReason(AccessRights.PastReports);

        protected override async Task OnInitializedAsync()
        {
            Department = await Task.Run(() => DepartmentServices.GetById(DepartmentId, Context));
            if (Department is null) return;

            LoadConfig();
            Nurses = await Task.Run(() => LoadNurses());
            ReportDate = Calendar.Today;
            PickDate = ReportDate;
            await LoadReportAsync();
        }

        /// <summary>
        /// Ширину селектора дежурной м/с подгоняем под самое длинное имя списка,
        /// ограничивая свободным местом строки. Замер делает JS (wwwroot/js/dutyNurse.js).
        /// </summary>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            await JsRuntime.InvokeVoidAsync("dutyNurseWidth", "dutyNurse");
        }

        /// <summary>
        /// Медсёстры справочника, отсортированные для селектора: сначала своего отделения,
        /// затем чужие; внутри обеих групп — по алфавиту.
        /// </summary>
        private List<Personel> LoadNurses()
        {
            return PersonelService.GetByPersType("Медсестра")
                .OrderByDescending(n => BelongsToDepartment(n))
                .ThenBy(n => n.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Принадлежит ли сотрудник справочника текущему отделению. Значение в справочнике
        /// произвольный текст: сопоставляем по названию отделения или по числу в начала строки
        /// («7 отделение» → 7), как в старых записях справочника.
        /// </summary>
        private bool BelongsToDepartment(Personel person)
        {
            if (Department is null) return false;

            var departmentText = (person.Department ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(departmentText)) return false;

            if (string.Equals(departmentText, Department.Name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var digits = new string(departmentText.TakeWhile(char.IsDigit).ToArray());
            if (digits.Length == 0 || !int.TryParse(digits, out var number)) return false;

            return number == Department.Id || number == Department.Allias;
        }

        /// <summary>
        /// Чтение настроек приложения (видимость строк, readonly-поля)
        /// </summary>
        private void LoadConfig()
        {
            NozologyName = GetConfig("Nozology:Name", "");
            NozologyVisible = GetConfigBool("Nozology:Visible", false);

            OxygenName = GetConfig("Oxygen:Name", "");
            OxygenVisible = GetConfigBool("Oxygen:Visible", false);

            SocialName = GetConfig("Social:Name", "");
            SocialVisible = GetConfigBool("Social:Visible", false);

            CareName = GetConfig("Care:Name", "");
            CareVisible = GetConfigBool("Care:Visible", false);
        }

        /// <summary>
        /// Открытие сводки за дату, выбранную в календаре, по паролю.
        /// </summary>
        private async Task Unlock()
        {
            UnlockError = null;

            if (PickDate is null)
            {
                UnlockError = "Выберите дату в календаре.";
                return;
            }

            if (PickDate > Calendar.Today)
            {
                UnlockError = "Дата позже текущих отчётных суток. Будущие сутки ещё не закрыты.";
                return;
            }

            if (PickDate < MinReportDate)
            {
                UnlockError = $"Слишком ранняя дата. Доступны сутки не раньше {MinPickDate}.";
                return;
            }

            if (!AccessControl.IsGranted(AccessRights.PastReports, UnlockPassword))
            {
                UnlockError = "Неверный пароль.";
                UnlockPassword = string.Empty;
                return;
            }

            UnlockPassword = string.Empty;

            ReportDate = PickDate.Value;
            await LoadReportAsync();
        }

        /// <summary>
        /// Возврат к сводке за текущие отчётные сутки
        /// </summary>
        private async Task GoToToday()
        {
            if (!IsPastDay) return;

            UnlockError = null;
            PickDate = Calendar.Today;

            ReportDate = Calendar.Today;
            await LoadReportAsync();
        }

        /// <summary>
        /// Загрузка отчёта отделения за отчётные сутки
        /// </summary>
        private async Task LoadReportAsync()
        {
            Report = await Task.Run(() => DepReportService.GetByNumber(DepartmentId, ReportDate));

            if (Report is null)
            {
                Report = new DepReportEntity { depNumber = DepartmentId, reportDate = ReportDate };
            }

            // иначе сохранение затрёт дежурную м/с пустым значением
            DutyNurse = Report.dutyNurse ?? string.Empty;
        }

        // --- отклик на успешное сохранение ---
        /// <summary>По кнопке «Сохранить» идёт полоса слева направо: сводка записана.</summary>
        private bool SaveSweep { get; set; }

        /// <summary>
        /// Сохранение отчёта в БД
        /// </summary>
        private async Task Save()
        {
            if (Report is null) return;
            Report.dutyNurse = DutyNurse;
            // «Состоит» — вычисляемое поле. В упрощённой форме (КДО, платные услуги)
            // present — это «Обратилось», его вводят руками, поэтому формула не применяется.
            if (!IsSimpleForm)
            {
                Report.present = PresentComputed;
                Report.presentChildrens = PresentChildrensComputed;
            }
            // Отчётные сутки задаются формой и не пересчитываются при сохранении,
            // поэтому одни и те же сутки находятся при любом времени нажатия.
            // date — только отметка о фактическом времени сохранения.
            Report.reportDate = ReportDate;
            Report.date = Calendar.LocalNow;
            await Task.Run(() => DepReportService.AddOrUpdate(Report));

            // Отклик только после успешной записи: полоса идёт 1 секунду, затем класс снимается.
            // StateHasChanged обязателен: Blazor не перерисовывает компонент между await
            // внутри обработчика, и без него отклик появился бы только после всего метода.
            SaveSweep = true;
            await InvokeAsync(StateHasChanged);
            await Task.Delay(1000);
            SaveSweep = false;
        }

        /// <summary>
        /// Загрузка данных предыдущих суток в текущий отчёт.
        /// Доступно только для текущих отчётных суток: сводки за прошлые даты не перезаписываются.
        /// Упрощённой форме (КДО, платные услуги) недоступно: у неё нет «Состояло»/«Состоит»,
        /// а «Обратилось» вводится руками и из предыдущей сводки не переносится.
        /// </summary>
        private async Task LoadPrevious()
        {
            if (IsPastDay || IsSimpleForm) return;

            var prevDay = ReportDate.AddDays(-1);

            var lastReport = await Task.Run(() => DepReportService.GetByNumberNoTracking(DepartmentId, prevDay));
            if (lastReport is null)
            {
                return;
            }

            var currentReport = await Task.Run(() => DepReportService.GetByNumber(DepartmentId, ReportDate));

            // Сводка за текущие сутки уже заполнена — предупреждаем, что значения будут заменены.
            if (currentReport is not null
                && !await JsRuntime.InvokeAsync<bool>("confirm",
                    "Загрузить данные за вчера?\nВведённые за текущие сутки значения будут заменены."))
            {
                return;
            }

            // База в обоих случаях — вчерашняя сводка: состояние и нозологии из неё,
            // движение обнуляется под заполнение за сегодня.
            var rewritten = DepReportService.RewriteReportForLastDay(lastReport);
            rewritten.depNumber = DepartmentId;
            rewritten.reportDate = ReportDate;
            rewritten.date = Calendar.LocalNow;

            if (currentReport is null)
            {
                rewritten.Id = 0;
                await Task.Run(() => DepReportService.AddOrUpdate(rewritten));
            }
            else
            {
                rewritten.Id = currentReport.Id;
                await Task.Run(() => DepReportService.UpdateTracked(currentReport, rewritten));
            }

            await LoadReportAsync();
        }

        /// <summary>
        /// Печать страницы
        /// </summary>
        private async Task Print()
        {
            await JsRuntime.InvokeVoidAsync("window.print");
        }

        private string GetConfig(string key, string defaultValue)
        {
            var value = AppConfig[key];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        private bool GetConfigBool(string key, bool defaultValue)
        {
            return bool.TryParse(AppConfig[key], out var result) ? result : defaultValue;
        }
    }
}