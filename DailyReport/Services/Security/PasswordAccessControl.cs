using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace DailyReport.Services.Security
{
    /// <summary>
    /// Проверка доступа по паролю из секции "Security" конфигурации.
    /// Сейчас это единственный способ подтверждения права; при переходе на ASP.NET Identity
    /// реализация заменяется, контракт <see cref="IAccessControl"/> остаётся прежним.
    /// </summary>
    public class PasswordAccessControl : IAccessControl
    {
        private const string NotConfiguredReason =
            "Пароль доступа к сводкам за прошлые даты не задан в конфигурации (секция Security).";

        private readonly SecurityOptions _options;

        public PasswordAccessControl(IOptions<SecurityOptions> options)
        {
            _options = options.Value;
        }

        public bool IsGranted(string right, string? secret)
        {
            if (string.IsNullOrEmpty(secret))
            {
                return false;
            }

            string? expected = right switch
            {
                AccessRights.PastReports => _options.PastReportPassword,
                _ => null
            };

            // пароль не настроен — право не выдаём ни при каком вводе
            if (string.IsNullOrEmpty(expected))
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(secret));
        }

        public string? GetUnavailableReason(string right)
        {
            return right switch
            {
                AccessRights.PastReports when string.IsNullOrEmpty(_options.PastReportPassword)
                    => NotConfiguredReason,
                AccessRights.PastReports => null,
                _ => "Неизвестное право доступа."
            };
        }
    }
}
