namespace DailyReport.Services.Security
{
    /// <summary>
    /// Проверка права доступа по предъявленному секрету.
    /// Единственное место в приложении, где решается, чем подтверждается право,
    /// поэтому переход на другой способ (ASP.NET Identity, роли) не затрагивает страницы.
    /// </summary>
    public interface IAccessControl
    {
        /// <summary>
        /// Подтверждено ли право доступа предъявленным секретом.
        /// </summary>
        /// <param name="right">Право из <see cref="AccessRights"/>.</param>
        /// <param name="secret">Предъявленный секрет (сейчас — пароль).</param>
        bool IsGranted(string right, string? secret);

        /// <summary>
        /// Причина, по которой право невозможно выдать, или null, если право можно получить.
        /// Нужен интерфейсу, чтобы объяснить пользователю причину, не зная настроек.
        /// </summary>
        /// <param name="right">Право из <see cref="AccessRights"/>.</param>
        string? GetUnavailableReason(string right);
    }
}
