using DailyReport.Models;
using DailyReport.Models.WardsModels;
using Microsoft.EntityFrameworkCore;

namespace DailyReport.Services.Wards
{
    /// <summary>
    /// Пациенты палат (разрез 3), перенос V1-сервиса
    /// <c>Services/WardServices/PatientServices.cs</c> на EF-сервис.
    ///
    /// Ключевые отличия от V1:
    /// - связь пациент ↔ палата — по паре (Department, WardNumber), сравнение в памяти
    ///   (<c>OrdinalIgnoreCase</c>): SQLite не приводит регистр кириллицы;
    /// - CRUD возвращает <c>bool/out error</c> для показа сообщений в UI, как в <see cref="WardService"/>;
    /// - <c>Age</c>/<c>AgeMonts</c>/<c>IsDisodered</c> при правке НЕ трогаем: в форме они
    ///   не показываются и не редактируются, остаются как есть в записи;
    /// - отслеживаемый <c>FirstOrDefault</c> + правка полей (конвенция сервисов V2), без
    ///   прикрепления копий через <c>Update</c>.
    ///
    /// Дата попадания в палату задаётся формой (при создании — текущая календарная дата);
    /// сервис сам её не подставляет, но на создании отклоняет пустую.
    /// </summary>
    public class PatientService
    {
        private readonly ApplicationContext _context;

        public PatientService(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>Пациенты отделения: сортировка по номеру палаты, затем по Id (как в V1).</summary>
        public List<Patient> GetPatientsByDepartment(int departmentId)
        {
            return _context.Patients
                .AsNoTracking()
                .Where(p => p.Department == departmentId)
                .OrderBy(p => p.WardNumber)
                .ThenBy(p => p.Id)
                .ToList();
        }

        /// <summary>Пациенты палаты отделения (сравнение номера в памяти).</summary>
        public List<Patient> GetPatientsInWard(int departmentId, string wardNumber)
        {
            var number = (wardNumber ?? string.Empty).Trim();

            return GetPatientsByDepartment(departmentId)
                .Where(p => string.Equals((p.WardNumber ?? string.Empty).Trim(), number, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public Patient? GetPatient(int id)
        {
            return _context.Patients.AsNoTracking().FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Оформление пациента в палату. Проверяются ФИО, номер палаты, отделение,
        /// дата попадания и наличие самой палаты в отделении.
        /// </summary>
        public bool Add(Patient patient, out string error)
        {
            error = string.Empty;

            if (!Validate(patient, out error))
            {
                return false;
            }

            patient.Name = patient.Name.Trim();
            patient.WardNumber = patient.WardNumber.Trim();

            if (!WardExists(patient.Department, patient.WardNumber))
            {
                error = $"Палата «{patient.WardNumber}» в этом отделении не найдена.";
                return false;
            }

            _context.Patients.Add(patient);
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Правка пациента. <c>Age</c>/<c>AgeMonts</c>/<c>IsDisodered</c> сохраняются
        /// исходными — в форме они не редактируются.
        /// </summary>
        public bool Update(Patient patient, out string error)
        {
            error = string.Empty;

            if (patient.Id == 0)
            {
                error = "Не выбран пациент.";
                return false;
            }

            if (!Validate(patient, out error))
            {
                return false;
            }

            var stored = _context.Patients.FirstOrDefault(p => p.Id == patient.Id);
            if (stored is null)
            {
                error = "Пациент не найден.";
                return false;
            }

            var newNumber = patient.WardNumber.Trim();
            var moved = stored.Department != patient.Department
                || !string.Equals((stored.WardNumber ?? string.Empty).Trim(), newNumber, StringComparison.OrdinalIgnoreCase);

            if (moved && !WardExists(patient.Department, newNumber))
            {
                error = $"Палата «{newNumber}» в этом отделении не найдена.";
                return false;
            }

            stored.Name = patient.Name.Trim();
            stored.WardNumber = newNumber;
            stored.Department = patient.Department;
            stored.Male = patient.Male;
            stored.sAge = patient.sAge;
            stored.Diagnos = patient.Diagnos;
            stored.HospitalisationDate = patient.HospitalisationDate;
            stored.HasRash = patient.HasRash;
            stored.HasCareRisk = patient.HasCareRisk;
            stored.IsUntochable = patient.IsUntochable;

            _context.SaveChanges();
            return true;
        }

        /// <summary>Удаление пациента (подтверждение — на стороне UI, как в V1).</summary>
        public bool Delete(int id, out string error)
        {
            error = string.Empty;

            var stored = _context.Patients.FirstOrDefault(p => p.Id == id);
            if (stored is null)
            {
                error = "Пациент не найден.";
                return false;
            }

            _context.Patients.Remove(stored);
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Перенос пациента в другую палату того же отделения (drag-n-drop): меняется только
        /// <c>WardNumber</c>, <c>Department</c> не меняется. Перенос в закрытую/полную палату
        /// здесь НЕ запрещён — решение об этом принимает UI (с подтверждением).
        /// </summary>
        public bool MoveToWard(int patientId, string wardNumber, out string error)
        {
            error = string.Empty;
            wardNumber = (wardNumber ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(wardNumber))
            {
                error = "Укажите номер палаты.";
                return false;
            }

            var stored = _context.Patients.FirstOrDefault(p => p.Id == patientId);
            if (stored is null)
            {
                error = "Пациент не найден.";
                return false;
            }

            if (!WardExists(stored.Department, wardNumber))
            {
                error = $"Палата «{wardNumber}» в этом отделении не найдена.";
                return false;
            }

            stored.WardNumber = wardNumber;
            _context.SaveChanges();
            return true;
        }

        private static bool Validate(Patient patient, out string error)
        {
            if (string.IsNullOrWhiteSpace(patient.Name))
            {
                error = "Укажите ФИО пациента.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(patient.WardNumber))
            {
                error = "Укажите номер палаты.";
                return false;
            }

            if (patient.Department <= 0)
            {
                error = "Не выбрано отделение.";
                return false;
            }

            if (patient.HospitalisationDate == default)
            {
                error = "Укажите дату попадания в палату.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        /// <summary>Есть ли в отделении палата с таким номером (без учёта регистра и пробелов).</summary>
        private bool WardExists(int departmentId, string wardNumber)
        {
            var numbers = _context.Wards
                .AsNoTracking()
                .Where(w => w.Department == departmentId)
                .Select(w => w.Number)
                .ToList();

            return numbers.Any(n => string.Equals((n ?? string.Empty).Trim(), wardNumber, StringComparison.OrdinalIgnoreCase));
        }
    }
}
