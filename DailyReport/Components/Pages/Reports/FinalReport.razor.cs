using System.Globalization;
using DailyReport.Models;
using DailyReport.Models.PersonelFolder;
using DailyReport.Models.Reports;
using DailyReport.Services.Personnel;
using DailyReport.Services.Reports;
using DailyReport.Services.Reporting;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;

namespace DailyReport.Components.Pages.Reports
{
    /// <summary>
    /// Итоговая сводка по больнице за одни отчётные сутки (раздел 1).
    /// Таблица сводки только читается: данные берутся из сводок отделений и здесь не сохраняются.
    /// Исключение — блок «Дежурная смена» под таблицей, он ведётся прямо здесь, как в V1,
    /// и только за текущие отчётные сутки.
    /// </summary>
    public partial class FinalReport
    {
        /// <summary>Блоки дежурной смены под таблицей сводки, в порядке вывода.</summary>
        public static readonly DutyBlockInfo[] DutyBlocks =
        {
            new(DutyType.Department, "Врачи приёмного отделения", string.Empty),
            new(DutyType.Reanimanion, "ОРИТ", "ОРИТ"),
            new(DutyType.Rentgenology, "Рентгенологи (КТ)", "Рентгенология")
        };

        [Inject] private SummaryService SummaryService { get; set; } = null!;
        [Inject] private ReportingCalendar Calendar { get; set; } = null!;
        [Inject] private DutyService DutyService { get; set; } = null!;
        [Inject] private PersonelService PersonelService { get; set; } = null!;
        [Inject] private OutPatientService OutPatientService { get; set; } = null!;
        [Inject] private IConfiguration AppConfig { get; set; } = null!;
        [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
        [Inject] private NavigationManager Nav { get; set; } = null!;

        /// <summary>Собранная сводка за открытые отчётные сутки</summary>
        public FinalReportData? Data { get; set; }

        /// <summary>Отчётные сутки открытой сводки</summary>
        public DateOnly ReportDate { get; set; }

        public IReadOnlyList<SummarySection> Sections => SummaryRowCatalog.Sections;

        /// <summary>Дежурные за открытые сутки, все три блока вместе.</summary>
        public IReadOnlyList<DutyDoc> DutyDocs { get; private set; } = Array.Empty<DutyDoc>();

        /// <summary>Обычные врачи — для блоков «Врачи приёмного отделения» и «ОРИТ».</summary>
        public IReadOnlyList<Personel> Doctors { get; private set; } = Array.Empty<Personel>();

        /// <summary>Рентгенологи — для блока «Рентгенологи (КТ)».</summary>
        public IReadOnlyList<Personel> Rentgenologists { get; private set; } = Array.Empty<Personel>();

        /// <summary>Текст последней ошибки блока дежурных.</summary>
        public string? DutyError { get; set; }

        /// <summary>Черновики формы добавления: по одному на блок.</summary>
        public DutyDraft DraftDoctors { get; } = new();

        public DutyDraft DraftOrit { get; } = new();

        public DutyDraft DraftRentgenology { get; } = new();

        /// <summary>
        /// Дежурных можно править только за текущие отчётные сутки. Пароля на сводке нет,
        /// поэтому за прошлые даты блок остаётся для чтения — иначе история переписывалась бы
        /// без пароля, в отличие от страницы отделения.
        /// </summary>
        public bool CanEditDuty => !IsPastDay;

        /// <summary>
        /// Отказы от госпитализации за открытые сутки. Это те же сутки, что у дежурных:
        /// правило правки одно, пароля на сводке нет.
        /// </summary>
        public IReadOnlyList<OutcomingPatient> OutPatients { get; private set; } = Array.Empty<OutcomingPatient>();

        /// <summary>
        /// Числа четырёх строк сводки «Отказ от госпитализации» и следующих. Считаются
        /// из списка <see cref="OutPatients"/>, отделение в них не участвует.
        /// </summary>
        public OutRejectCounts RejectCounts { get; private set; } = new();

        /// <summary>Текст последней ошибки блока отказов.</summary>
        public string? OutError { get; set; }

        /// <summary>Черновик формы добавления пациента.</summary>
        public OutPatientDraft OutDraft { get; } = new();

        /// <summary>Правка разрешена только за текущие отчётные сутки — как у дежурных.</summary>
        public bool CanEditOutPatients => !IsPastDay;

        /// <summary>
        /// Значение-маркер «свой вариант» в списках. В модель не попадает:
        /// вместо него в поле пишется то, что оператор ввёл руками.
        /// </summary>
        public const string CustomOption = OutFieldSelect.CustomOption;

        /// <summary>Поля блока с выбором из списка. Порядок совпадает с колонками таблицы.</summary>
        public enum OutField
        {
            Shipped,
            SubmitedFrom,
            SubmitedTo
        }

        private string[]? shippedOptions;
        private string[]? submitedFromOptions;
        private string[]? submitedToOptions;

        /// <summary>Кем доставлен — значения из конфига OutPatients:Shipped.</summary>
        public string[] ShippedOptions => shippedOptions ??= ReadOutList("Shipped");

        /// <summary>«Самообращение» — первый пункт списка «Направлен из» (решение пользователя 07.10.2026).</summary>
        public const string SelfReferralFrom = "самообращение";

        /// <summary>
        /// Откуда направлен: сначала «самообращение», потом поликлиники, потом больницы —
        /// по решению пользователя, в том порядке, в каком список пришёл в задачу.
        /// </summary>
        public string[] SubmitedFromOptions => submitedFromOptions ??=
            new[] { SelfReferralFrom }
                .Concat(ReadOutList("Clinics"))
                .Concat(ReadOutList("Hospitals"))
                .ToArray();

        /// <summary>Куда переведён — только больницы (OutPatients:Hospitals).</summary>
        public string[] SubmitedToOptions => submitedToOptions ??= ReadOutList("Hospitals");

        /// <summary>Список для поля: от него зависит и селект, и проверка «свой вариант».</summary>
        public string[] OutOptions(OutField field) => field switch
        {
            OutField.Shipped => ShippedOptions,
            OutField.SubmitedFrom => SubmitedFromOptions,
            _ => SubmitedToOptions
        };

        /// <summary>
        /// Флаги «оператор выбрал свой вариант» по строке списка. В модели их нет:
        /// это состояние экрана, поэтому после каждого перечитывания сбрасывается.
        /// </summary>
        private readonly Dictionary<int, OutCustomFlags> outCustom = new();

        private OutCustomFlags GetOutCustom(int id)
        {
            if (!outCustom.TryGetValue(id, out var flags))
            {
                outCustom[id] = flags = new OutCustomFlags();
            }

            return flags;
        }

        /// <summary>
        /// Считается ли значение поля ручным: либо явно выбран «свой вариант»,
        /// либо текста нет в списке (старые строки вроде «гб4» — показываем как есть).
        /// </summary>
        private bool IsCustom(string? value, bool customFlag, OutField field)
            => customFlag || (!string.IsNullOrWhiteSpace(value)
                && Array.IndexOf(OutOptions(field), value) < 0);

        /// <summary>Что показывает селект строки списка: маркер или сохранённое значение.</summary>
        public string OutSelectValue(OutcomingPatient patient, OutField field)
        {
            var flags = outCustom.TryGetValue(patient.Id, out var found) ? found : null;
            var value = GetValue(patient, field);
            var flagCustom = flags is not null && GetFlag(flags, field);

            return IsCustom(value, flagCustom, field) ? CustomOption : value ?? string.Empty;
        }

        /// <summary>Что показывает селект формы добавления.</summary>
        public string OutDraftSelectValue(OutField field)
        {
            var value = GetDraftValue(field);
            return IsCustom(value, GetDraftFlag(field), field) ? CustomOption : value ?? string.Empty;
        }

        /// <summary>Показывать ли под селектом строки ручное поле.</summary>
        public bool OutFieldCustom(OutcomingPatient patient, OutField field)
        {
            var flags = outCustom.TryGetValue(patient.Id, out var found) ? found : null;
            return IsCustom(GetValue(patient, field), flags is not null && GetFlag(flags, field), field);
        }

        /// <summary>Показывать ли под селектом формы добавления ручное поле.</summary>
        public bool OutDraftFieldCustom(OutField field)
            => IsCustom(GetDraftValue(field), GetDraftFlag(field), field);

        /// <summary>Правка селекта в строке списка: либо значение из списка, либо свой вариант.</summary>
        private void SelectOutField(OutcomingPatient patient, OutField field, ChangeEventArgs e)
        {
            if (!CanEditOutPatients) return;

            var flags = GetOutCustom(patient.Id);
            var selected = e.Value?.ToString();

            if (selected == CustomOption)
            {
                SetFlag(flags, field, true);

                // Текст из списка освобождаем под ручной ввод, а своё старое значение
                // («гб4», «АМБУЛАТОРНО») оставляем — его удалять нельзя.
                var value = GetValue(patient, field);

                if (!string.IsNullOrWhiteSpace(value) && Array.IndexOf(OutOptions(field), value) >= 0)
                {
                    SetValue(patient, field, null);
                }

                return;
            }

            SetFlag(flags, field, false);
            SetValue(patient, field, selected);
        }

        /// <summary>Правка селекта в форме добавления.</summary>
        private void SelectOutDraftField(OutField field, ChangeEventArgs e)
        {
            var selected = e.Value?.ToString();

            if (selected == CustomOption)
            {
                SetDraftFlag(field, true);

                var value = GetDraftValue(field);

                if (!string.IsNullOrWhiteSpace(value) && Array.IndexOf(OutOptions(field), value) >= 0)
                {
                    SetDraftValue(field, null);
                }

                return;
            }

            SetDraftFlag(field, false);
            SetDraftValue(field, selected);
        }

        /// <summary>Значение поля модели.</summary>
        private static string? GetValue(OutcomingPatient patient, OutField field) => field switch
        {
            OutField.Shipped => patient.Shipped,
            OutField.SubmitedFrom => patient.SubmitedFrom,
            _ => patient.SubmitedTo
        };

        private static void SetValue(OutcomingPatient patient, OutField field, string? value)
        {
            switch (field)
            {
                case OutField.Shipped: patient.Shipped = value; break;
                case OutField.SubmitedFrom: patient.SubmitedFrom = value; break;
                default: patient.SubmitedTo = value; break;
            }
        }

        /// <summary>Значение поля черновика.</summary>
        private string? GetDraftValue(OutField field) => field switch
        {
            OutField.Shipped => OutDraft.Shipped,
            OutField.SubmitedFrom => OutDraft.SubmitedFrom,
            _ => OutDraft.SubmitedTo
        };

        private void SetDraftValue(OutField field, string? value)
        {
            switch (field)
            {
                case OutField.Shipped: OutDraft.Shipped = value; break;
                case OutField.SubmitedFrom: OutDraft.SubmitedFrom = value; break;
                default: OutDraft.SubmitedTo = value; break;
            }
        }

        private static bool GetFlag(OutCustomFlags flags, OutField field) => field switch
        {
            OutField.Shipped => flags.Shipped,
            OutField.SubmitedFrom => flags.SubmitedFrom,
            _ => flags.SubmitedTo
        };

        private static void SetFlag(OutCustomFlags flags, OutField field, bool value)
        {
            switch (field)
            {
                case OutField.Shipped: flags.Shipped = value; break;
                case OutField.SubmitedFrom: flags.SubmitedFrom = value; break;
                default: flags.SubmitedTo = value; break;
            }
        }

        private bool GetDraftFlag(OutField field) => field switch
        {
            OutField.Shipped => OutDraft.ShippedCustom,
            OutField.SubmitedFrom => OutDraft.SubmitedFromCustom,
            _ => OutDraft.SubmitedToCustom
        };

        private void SetDraftFlag(OutField field, bool value)
        {
            switch (field)
            {
                case OutField.Shipped: OutDraft.ShippedCustom = value; break;
                case OutField.SubmitedFrom: OutDraft.SubmitedFromCustom = value; break;
                default: OutDraft.SubmitedToCustom = value; break;
            }
        }

        /// <summary>Флаги «свой вариант» одной строки списка.</summary>
        private sealed class OutCustomFlags
        {
            public bool Shipped { get; set; }

            public bool SubmitedFrom { get; set; }

            public bool SubmitedTo { get; set; }
        }

        /// <summary>
        /// Список значений из секции OutPatients конфигурации. Читается по индексам
        /// (OutPatients:Shipped:0, …): секции нет — возвращается пустой список,
        /// форма остаётся только со «своим вариантом».
        /// </summary>
        private string[] ReadOutList(string key)
        {
            var list = new List<string>();

            for (var i = 0; ; i++)
            {
                var item = AppConfig[$"OutPatients:{key}:{i}"];

                if (item is null)
                {
                    break;
                }

                if (!string.IsNullOrWhiteSpace(item))
                {
                    list.Add(item.Trim());
                }
            }

            return list.ToArray();
        }

        /// <summary>Маршруты для выбора в форме, в порядке строк сводки.</summary>
        public static readonly OutRoute[] OutRoutes =
        {
            OutRoute.Reject,
            OutRoute.Ambulatory,
            OutRoute.OtherHospital
        };

        /// <summary>Подпись маршрута в форме и в печатном списке.</summary>
        public static string RouteTitle(OutRoute route) => route switch
        {
            OutRoute.Reject => "Отказ от госпитализации",
            OutRoute.Ambulatory => "Амбулаторно",
            _ => "В другую МО"
        };

        /// <summary>
        /// Подпись маршрута для записи из базы: у старых строк тип выводится по тексту
        /// «Переведен», а не выбирался формой.
        /// </summary>
        public static string RouteTitleOf(OutcomingPatient patient)
            => OutPatientService.ResolveRoute(patient) is { } route ? RouteTitle(route) : string.Empty;

        /// <summary>
        /// Возраст подписью для печати: «34 года» или «1 год 6 мес». Пустые части
        /// пропускаются, чтобы в графе не оставалось вида «  мес».
        /// </summary>
        public static string AgeText(string? years, string? months)
        {
            var hasYears = !string.IsNullOrWhiteSpace(years);
            var hasMonths = !string.IsNullOrWhiteSpace(months);

            if (!hasYears && !hasMonths)
            {
                return string.Empty;
            }

            if (hasYears && !hasMonths)
            {
                return $"{years!.Trim()} {YearsWord(years)}";
            }

            if (!hasYears)
            {
                return $"{months!.Trim()} мес";
            }

            return $"{years!.Trim()} {YearsWord(years)} {months!.Trim()} мес";
        }

        private static string YearsWord(string? years)
        {
            var text = (years ?? string.Empty).Trim().Replace(',', '.');

            if (!double.TryParse(text, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var value))
            {
                return "лет";
            }

            var whole = (int)Math.Floor(value);
            var mod10 = whole % 10;

            if (whole is >= 11 and <= 14)
            {
                return "лет";
            }

            return mod10 switch
            {
                1 => "год",
                2 or 3 or 4 => "года",
                _ => "лет"
            };
        }

        /// <summary>Число ячеек таблицы: подпись, колонки отделений и шесть итоговых колонок</summary>
        private int TotalColumns => Data is null ? 1 : 1 + Data.Columns.Count * 2 + 6;

        /// <summary>Открыты не текущие отчётные сутки.</summary>
        public bool IsPastDay => ReportDate != Calendar.Today;

        /// <summary>Отчётные сутки, выбранные в календаре. null — дата не выбрана.</summary>
        public DateOnly? PickDate { get; set; }

        /// <summary>Текст последней ошибки выбора даты.</summary>
        public string? OffsetError { get; set; }

        /// <summary>
        /// Верхняя граница календаря: текущие отчётные сутки. Будущее выбрать нельзя.
        /// </summary>
        public string MaxPickDate => Calendar.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        /// <summary>Нижняя граница календаря: глубина перехода не ограничена.</summary>
        public string MinPickDate => MinReportDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        /// <summary>
        /// Самые ранние отчётные сутки, доступные для выбора. Дно нужно, чтобы календарь
        /// не позволял уйти в даты, которых не бывает, и чтобы не поймать переполнение.
        /// </summary>
        private static DateOnly MinReportDate => DateOnly.FromDateTime(new DateTime(2000, 1, 1));

        protected override async Task OnInitializedAsync()
        {
            ReportDate = Calendar.Today;
            PickDate = ReportDate;

            // Ссылка «Сводка для планерки» с главной страницы приходит с датой в адресе:
            // /finalreport?date=2026-10-03. Параметр добровольный, без него сводка,
            // как и раньше, открывается за текущие отчётные сутки.
            if (ReadDateFromUrl() is { } requested && IsDateAllowed(requested))
            {
                ReportDate = requested;
                PickDate = requested;
            }

            // Списки дежурных берём из справочника персонала через DutyService: какая
            // должность попадает в какой блок — правило службы, а не страницы.
            Doctors = await Task.Run(() => PersonelService.GetByPersType(DutyService.GetPersTypeFor(DutyType.Department)));
            Rentgenologists = await Task.Run(() => PersonelService.GetByPersType(DutyService.GetPersTypeFor(DutyType.Rentgenology)));

            await LoadAsync();
        }

        /// <summary>Дата отчётных суток из адреса, null — параметра нет или он не разобран.</summary>
        private DateOnly? ReadDateFromUrl()
        {
            var query = QueryHelpers.ParseQuery(new Uri(Nav.Uri).Query);

            if (!query.TryGetValue("date", out var raw))
            {
                return null;
            }

            return DateOnly.TryParseExact(raw.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed
                : null;
        }

        /// <summary>
        /// Границы выбора даты те же, что у календаря внизу страницы: не раньше 01.01.2000
        /// и не позже текущих отчётных суток.
        /// </summary>
        private bool IsDateAllowed(DateOnly day) => day >= MinReportDate && day <= Calendar.Today;

        private async Task LoadAsync()
        {
            var config = new Dictionary<SummaryConfigGroup, SummaryRowConfig>
            {
                [SummaryConfigGroup.Oxygen] = ReadConfigGroup("Oxygen"),
                [SummaryConfigGroup.Social] = ReadConfigGroup("Social"),
                [SummaryConfigGroup.Nozology] = ReadConfigGroup("Nozology"),
                [SummaryConfigGroup.Care] = ReadConfigGroup("Care")
            };

            var day = ReportDate;
            Data = await Task.Run(() => SummaryService.Build(day, config));
            DutyDocs = await Task.Run(() => DutyService.GetForDay(day));
            ResetDutyDrafts();
            LoadOutPatients();
        }

        /// <summary>
        /// Перечитывает только дежурных за открытые сутки. Сводку не трогаем: она от
        /// дежурных не зависит, а пересборка после каждого сохранения мигала бы всей таблицей.
        /// </summary>
        private void LoadDuty()
        {
            DutyDocs = DutyService.GetForDay(ReportDate);
        }

        /// <summary>Черновик формы добавления для блока.</summary>
        public DutyDraft GetDraft(DutyType type) => type switch
        {
            DutyType.Reanimanion => DraftOrit,
            DutyType.Rentgenology => DraftRentgenology,
            _ => DraftDoctors
        };

        /// <summary>Кого можно выбрать дежурным в этом блоке.</summary>
        public IReadOnlyList<Personel> GetStaff(DutyType type) => type == DutyType.Rentgenology
            ? Rentgenologists
            : Doctors;

        /// <summary>
        /// Подставляет в форму добавления подразделение блока: у ОРИТ — «ОРИТ»,
        /// у рентгенологов — «Рентгенология», у линейных врачей поле остаётся пустым.
        /// </summary>
        private void ResetDutyDrafts()
        {
            foreach (var block in DutyBlocks)
            {
                GetDraft(block.Type).Departments = block.Department;
            }
        }

        private void AddDuty(DutyBlockInfo block)
        {
            if (!CanEditDuty) return;

            var draft = GetDraft(block.Type);
            var doc = new DutyDoc
            {
                dutyDoc = draft.Name,
                departments = draft.Departments,
                type = block.Type
            };

            if (DutyService.Add(doc, ReportDate, out var error))
            {
                draft.Name = null;
                draft.Departments = block.Department;
                DutyError = null;
            }
            else
            {
                DutyError = error;
            }

            LoadDuty();
        }

        private void UpdateDuty(DutyDoc doc)
        {
            if (!CanEditDuty) return;

            DutyError = DutyService.Update(doc, out var error) ? null : error;
            LoadDuty();
        }

        private void DeleteDuty(DutyDoc doc)
        {
            if (!CanEditDuty) return;

            DutyService.Delete(doc.Id);
            DutyError = null;
            LoadDuty();
        }

        /// <summary>
        /// Перечитывает отказы за открытые сутки и пересчитывает четыре строки сводки.
        /// Таблицу сводки не трогаем: кроме отказов она ни от чего в списке не зависит.
        /// </summary>
        private void LoadOutPatients()
        {
            OutPatients = OutPatientService.GetForDay(ReportDate);
            RejectCounts = OutPatientService.CountByRoute(OutPatients);
            outCustom.Clear();
        }

        private void AddOutPatient()
        {
            if (!CanEditOutPatients) return;

            var draft = OutDraft;
            var patient = new OutcomingPatient
            {
                Name = draft.Name,
                Gender = draft.Gender,
                AgeYears = draft.AgeYears,
                AgeMonth = draft.AgeMonth,
                Diagnos = draft.Diagnos,
                Shipped = draft.Shipped,
                SubmitedFrom = draft.SubmitedFrom,
                SubmitedTo = draft.SubmitedTo,
                Route = draft.Route
            };

            if (OutPatientService.Add(patient, ReportDate, out var error))
            {
                OutDraft.Clear();
                OutError = null;
            }
            else
            {
                OutError = error;
            }

            LoadOutPatients();
        }

        private void UpdateOutPatient(OutcomingPatient patient)
        {
            if (!CanEditOutPatients) return;

            OutError = OutPatientService.Update(patient, out var error) ? null : error;
            LoadOutPatients();
        }

        private void DeleteOutPatient(OutcomingPatient patient)
        {
            if (!CanEditOutPatients) return;

            OutPatientService.Delete(patient.Id);
            OutError = null;
            LoadOutPatients();
        }

        /// <summary>
        /// Подпись и видимость опциональной строки из секции конфигурации.
        /// </summary>
        private SummaryRowConfig ReadConfigGroup(string group)
        {
            var name = AppConfig[$"{group}:Name"];
            var visible = bool.TryParse(AppConfig[$"{group}:Visible"], out var result) && result;
            return new SummaryRowConfig(name ?? string.Empty, visible);
        }

        /// <summary>
        /// Открытие сводки за дату, выбранную в календаре. Пароля нет: сводка
        /// общедоступна, в отличие от формы отделения.
        /// </summary>
        private async Task GoToDate()
        {
            OffsetError = null;

            if (PickDate is null)
            {
                OffsetError = "Выберите дату в календаре.";
                return;
            }

            if (PickDate > Calendar.Today)
            {
                OffsetError = "Дата позже текущих отчётных суток. Будущие сутки ещё не закрыты.";
                return;
            }

            if (PickDate < MinReportDate)
            {
                OffsetError = $"Слишком ранняя дата. Доступны сутки не раньше {MinPickDate}.";
                return;
            }

            ReportDate = PickDate.Value;
            await LoadAsync();
        }

        private async Task GoToToday()
        {
            if (!IsPastDay) return;

            ReportDate = Calendar.Today;
            PickDate = ReportDate;
            OffsetError = null;

            await LoadAsync();
        }

        private async Task Print()
        {
            await JsRuntime.InvokeVoidAsync("window.print");
        }

        /// <summary>
        /// Ячейка отделения: ноль не печатается, как в сводке V1 — пустое значение
        /// не должно выглядеть как отчётные данные.
        /// </summary>
        private static string Cell(int value) => value > 0 ? value.ToString() : string.Empty;

        /// <summary>Описание блока дежурной смены: тип, заголовок и подразделение по умолчанию.</summary>
        public sealed record DutyBlockInfo(DutyType Type, string Title, string Department);

        /// <summary>Черновик строки для добавления дежурного.</summary>
        public sealed class DutyDraft
        {
            public string? Name { get; set; }

            public string? Departments { get; set; }
        }

        /// <summary>
        /// Черновик строки формы добавления отказа. Маршрут по умолчанию — отказ:
        /// это сая частая причина, по которой пациент попадает в этот список.
        /// </summary>
        public sealed class OutPatientDraft
        {
            public string? Name { get; set; }

            public string? Gender { get; set; }

            public string? AgeYears { get; set; }

            public string? AgeMonth { get; set; }

            public string? Diagnos { get; set; }

            public string? Shipped { get; set; }

            public string? SubmitedFrom { get; set; }

            public string? SubmitedTo { get; set; }

            /// <summary>Оператор выбрал «свой вариант» — под списком показывается поле ввода.</summary>
            public bool ShippedCustom { get; set; }

            public bool SubmitedFromCustom { get; set; }

            public bool SubmitedToCustom { get; set; }

            public OutRoute? Route { get; set; } = OutRoute.Reject;

            public void Clear()
            {
                Name = null;
                Gender = null;
                AgeYears = null;
                AgeMonth = null;
                Diagnos = null;
                Shipped = null;
                SubmitedFrom = null;
                SubmitedTo = null;
                ShippedCustom = false;
                SubmitedFromCustom = false;
                SubmitedToCustom = false;
                Route = OutRoute.Reject;
            }
        }
    }
}
