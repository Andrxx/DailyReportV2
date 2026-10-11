namespace DailyReport.Models
{
    /// <summary>
    /// Правила поведения отделения, выводимые из его роли (<see cref="DepartmentType"/>).
    /// Заменяет сравнения с числовыми алиасами конфигурации V1.
    /// </summary>
    public static class DepartmentTypeExtensions
    {
        /// <summary>Отделение заполняет сводку по пациентам (разрез 1)</summary>
        public static bool CollectsPatientReport(this DepartmentType type) =>
            type is DepartmentType.General
                or DepartmentType.DayCare
                or DepartmentType.KDO
                or DepartmentType.PayService
                or DepartmentType.ORIT
                or DepartmentType.ORITDirty;

        /// <summary>Упрощённая форма отчёта: единственная строка «Обратилось»</summary>
        public static bool IsSimplePatientForm(this DepartmentType type) =>
            type is DepartmentType.KDO or DepartmentType.PayService;

        /// <summary>ОРИТ: строки «Поступило», «Выбыло», «Прикреплено в ОРИТ» не заполняются</summary>
        public static bool IsOrit(this DepartmentType type) =>
            type is DepartmentType.ORIT or DepartmentType.ORITDirty;

        /// <summary>
        /// Долгохранимая пожарная сводка: значение задаётся один раз и берётся до тех пор,
        /// пока в него не внесут изменения (охрана, общежитие). Это решения пользователя
        /// от 08.10.2026: охрана — вариант 2, общежитие — «как есть по последней строке».
        /// Лаборатория сюда НЕ входит: её сводка задаётся на каждые сутки, как у обычных отделений.
        /// </summary>
        public static bool IsLongStoredFireReport(this DepartmentType type) =>
            type is DepartmentType.Security or DepartmentType.Utility;
    }
}
