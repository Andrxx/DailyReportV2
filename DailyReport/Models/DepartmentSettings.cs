using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyReport.Models
{
    /// <summary>
    /// Тип (роль) отделения. Определяет особое поведение на страницах вместо скалярных
    /// алиас-констант конфигурации (KDOallias, PayServiceAllias, DayCareAllias, ORITAllias и т.п.)
    /// </summary>
    public enum DepartmentType
    {
        General = 0,      // обычное лечебное отделение
        DayCare = 1,      // дневной стационар — отдельная строка в итоговой сводке
        KDO = 2,          // клинико-диагностическое отделение — панель, упрощённая форма
        PayService = 3,   // платные услуги — панель, упрощённая форма
        ORIT = 4,         // ОРИТ — койки считаются отдельно
        ORITDirty = 5,    // ОРИТ грязная зона
        Laboratory = 6,   // лаборатория
        Security = 7,     // охрана
        Xray = 8,         // рентгендиагностика
        Utility = 9       // технические/хозяйственные подразделения
    }

    /// <summary>
    /// Откуда брать данные для автоматического заполнения сводки отделения,
    /// если за отчётные сутки строки нет (отделение не работает в этот день).
    /// </summary>
    public enum AutoFillSource
    {
        /// <summary>Строго предыдущий отчётный день</summary>
        PreviousDay = 0,

        /// <summary>Последняя фактическая запись отделения (переживает праздники и пропуски)</summary>
        LastAvailable = 1
    }

    /// <summary>
    /// Конфигурация показа отделения. Связь 1:1 с Department (DepartmentId = PK и FK).
    /// Создаётся автоматически вместе с отделением, удаляется каскадно.
    /// Изолирована от модели Department — мигрируется и меняется отдельно.
    /// </summary>
    public class DepartmentSettings
    {
        [Key]
        [ForeignKey(nameof(Department))]
        public int DepartmentId { get; set; }

        public Department? Department { get; set; }

        /// <summary>Роль отделения (вместо алиас-констант конфигурации)</summary>
        public DepartmentType Type { get; set; } = DepartmentType.General;

        /// <summary>Показывать на главной странице (сводки отделений)</summary>
        public bool ShowOnMainPage { get; set; } = true;

        /// <summary>Показывать в колонках общей сводки (итоговая сводка)</summary>
        public bool ShowOnFinalReport { get; set; } = true;

        /// <summary>Учитывать койки в расчёте коечного фонда общей сводки</summary>
        public bool CountSpotsInFinalReport { get; set; } = true;

        /// <summary>Показывать в пожарной сводке (список и подсчёт)</summary>
        public bool ShowOnFireReport { get; set; } = true;

        /// <summary>Показывать на страницах размещения в палатах (index и administration)</summary>
        public bool ShowOnWards { get; set; } = true;

        /// <summary>Упрощённая форма сводки: только строка «Обратилось» (отделение без приёма пациентов)</summary>
        public bool SimpleReportForm { get; set; }

        /// <summary>
        /// Брать прошлые значения: если строки за текущие отчётные сутки нет, итоговая сводка
        /// создаёт её автоматически из предыдущей (отделение не работает в этот день — выходной,
        /// праздник, пропуск подачи). Пользователю править сводку из итоговой сводки по-прежнему нельзя.
        /// </summary>
        public bool AutoFillFromPrevious { get; set; }

        /// <summary>Источник автозаполнения: предыдущий день или последняя фактическая запись</summary>
        public AutoFillSource AutoFillSource { get; set; } = AutoFillSource.PreviousDay;
    }
}