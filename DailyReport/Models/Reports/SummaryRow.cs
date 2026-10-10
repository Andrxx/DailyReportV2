namespace DailyReport.Models.Reports
{
    /// <summary>
    /// Раздел итоговой сводки. Порядок перечисления задаёт порядок разделов в таблице.
    /// </summary>
    public enum SummarySection
    {
        Movement,
        Oxygen,
        SocialStatus,
        Nozology,
        Care
    }

    /// <summary>
    /// Именованная группа опциональных строк, читаемая из конфигурации.
    /// </summary>
    public enum SummaryConfigGroup
    {
        None,
        Oxygen,
        Social,
        Nozology,
        Care
    }

    /// <summary>
    /// Описание одного показателя итоговой сводки. Набор строк фиксирован в коде,
    /// значения берутся из полей отчёта отделения через селекторы.
    /// </summary>
    public sealed class SummaryRow
    {
        /// <summary>Подпись показателя. Для опциональных строк приходит из конфигурации.</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>Раздел сводки</summary>
        public SummarySection Section { get; init; }

        /// <summary>Группа конфигурации: подпись и видимость берутся из appsettings</summary>
        public SummaryConfigGroup ConfigGroup { get; init; } = SummaryConfigGroup.None;

        /// <summary>Строка-итог внутри раздела, выделяется подсветкой</summary>
        public bool IsSubtotal { get; init; }

        /// <summary>Строка занимает обе колонки отделения (взрослые и дети вместе)</summary>
        public bool IsMerged { get; init; }

        /// <summary>Значение для взрослых</summary>
        public Func<DepReport, int>? Adults { get; init; }

        /// <summary>Значение для детей</summary>
        public Func<DepReport, int>? Children { get; init; }

        /// <summary>Значение для объединённой строки</summary>
        public Func<DepReport, int>? Merged { get; init; }

        /// <summary>
        /// Попадает ли строка в блок итогов «С учетом дневного стационара».
        /// Дневной стационар ведёт только движение пациентов: кислород, соцстатус,
        /// нозологии и уход по нему не собираются, поэтому в этом блоке такие строки
        /// оставляются пустыми, а не повторяют первый блок.
        /// </summary>
        public bool CountsInDayCareTotal => Section == SummarySection.Movement;
    }

    /// <summary>
    /// Колонка итоговой сводки: отделение и его отчёт за отчётные сутки.
    /// </summary>
    public sealed class SummaryColumn
    {
        public required Department Department { get; init; }

        /// <summary>Отчёт отделения. Пустой объект, если сводка за сутки не заполнена.</summary>
        public required DepReport Report { get; init; }

        /// <summary>Отделение заполнило сводку за эти сутки</summary>
        public bool HasData { get; init; }

        /// <summary>Дневной стационар: его значения не входят в сумму стационара</summary>
        public bool IsDayCare => Department.Settings?.Type == DepartmentType.DayCare;

        public int Spots => Department.AdultSpotsQuantity + Department.ChildrenSpotsQuantity;

        /// <summary>Коечный фонд отделения у взрослых</summary>
        public int AdultSpots => Department.AdultSpotsQuantity;

        /// <summary>Коечный фонд отделения у детей</summary>
        public int ChildrenSpots => Department.ChildrenSpotsQuantity;
    }

    /// <summary>
    /// Значения одного показателя сводки по всем колонкам и по итогам.
    /// </summary>
    public sealed class SummaryRowValues
    {
        public required SummaryRow Row { get; init; }

        /// <summary>Значения по колонкам, порядок совпадает с порядком колонок сводки</summary>
        public required int[] Adults { get; init; }

        public int[] Children { get; init; } = Array.Empty<int>();
        public int[] Merged { get; init; } = Array.Empty<int>();

        public int AdultsTotal { get; init; }
        public int ChildrenTotal { get; init; }
        public int MergedTotal { get; init; }

        /// <summary>Добавка дневного стационара к итогу стационара</summary>
        public int DayCareAdults { get; init; }

        public int DayCareChildren { get; init; }
        public int DayCareMerged { get; init; }
    }

    /// <summary>
    /// Данные итоговой сводки за одни отчётные сутки.
    /// </summary>
    public sealed class FinalReportData
    {
        public required DateOnly ReportDate { get; init; }

        public required List<SummaryColumn> Columns { get; init; }

        public required List<SummaryRowValues> Rows { get; init; }

        /// <summary>Всего колонок с учётом двух колонок на каждое отделение</summary>
        public int ColumnCount => Columns.Count * 2;

        public int FilledDepartments => Columns.Count(c => c.HasData);
        public bool HasDayCare => Columns.Any(c => c.IsDayCare);

        /// <summary>Коечный фонд стационара без дневного стационара</summary>
        public int AdultSpots { get; init; }

        public int ChildrenSpots { get; init; }

        /// <summary>
        /// Койки взрослых по первому блоку итогов: все отделения сводки,
        /// кроме дневного стационара. Считается отдельно от <see cref="AdultSpots"/>,
        /// чтобы правило «ДС не входит в сумму стационара» не зависело от флага
        /// CountSpotsInFinalReport.
        /// </summary>
        public int BedsAdults { get; init; }

        /// <summary>Койки детские по первому блоку итогов</summary>
        public int BedsChildren { get; init; }

        /// <summary>Койки взрослые дневного стационара: в первый блок итогов не входят</summary>
        public int DayCareBedsAdults { get; init; }

        /// <summary>Койки детские дневного стационара</summary>
        public int DayCareBedsChildren { get; init; }

        /// <summary>Койки ОРИТ, считаются отдельно от круглосуточного фонда</summary>
        public int OritSpots { get; init; }

        /// <summary>Обратившиеся в КДО</summary>
        public int KdoVisits { get; init; }

        /// <summary>Обратившиеся за платными услугами</summary>
        public int PayServiceVisits { get; init; }
    }
}
