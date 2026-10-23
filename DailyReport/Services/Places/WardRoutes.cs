namespace DailyReport.Services.Places;

/// <summary>
/// Маршруты страниц палат. Новая витрина «Места(Самсон)» считается канонической,
/// старая «Места» остаётся аварийной. При переезде (см. AGENTS.md, «Будущий переезд»)
/// меняются только эти константы и два атрибута <c>@page</c> — навигация и ссылки
/// в компонентах остаются прежними.
/// </summary>
public static class WardRoutes
{
    /// <summary>Индекс новой витрины палат.</summary>
    public const string SamsonIndex = "/samson/places";

    /// <summary>Отделение новой витрины (шаблон для <see cref="SamsonDepartment"/>).</summary>
    public const string SamsonDepartmentTemplate = "/samson/places/{0}";

    /// <summary>Общая сводка палат по всем отделениям.</summary>
    public const string SamsonDashboard = "/samson/dashboard";

    /// <summary>Индекс старой страницы «Места» (аварийный режим).</summary>
    public const string LegacyIndex = "/places";

    /// <summary>Отделение старой страницы «Места» (шаблон для <see cref="LegacyDepartment"/>).</summary>
    public const string LegacyDepartmentTemplate = "/places/{0}";

    public static string SamsonDepartment(int departmentId) =>
        string.Format(SamsonDepartmentTemplate, departmentId);

    public static string LegacyDepartment(int departmentId) =>
        string.Format(LegacyDepartmentTemplate, departmentId);
}
