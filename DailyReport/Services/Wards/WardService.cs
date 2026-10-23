using System.Globalization;
using DailyReport.Models;
using DailyReport.Models.WardsModels;
using DailyReport.Services.Places;
using Microsoft.EntityFrameworkCore;

namespace DailyReport.Services.Wards
{
    /// <summary>
    /// Палаты лечебных отделений (разрез 3), перенос V1-сервиса
    /// <c>Services/WardServices/WardServices.cs</c> на EF-сервис.
    ///
    /// Ключевые отличия от V1:
    /// - номер палаты уникален В ПРЕДЕЛАХ отделения (в V1 этого не проверялось);
    /// - переименование номера КАСКАДНО обновляет WardNumber пациентов палаты
    ///   в одной транзакции (иначе пациенты «отвязываются» от палаты);
    /// - сортировка номеров кастомная: числовые по возрастанию, затем именованные
    ///   по алфавиту (простой orderby Number поставил бы «10» раньше «2»).
    ///
    /// Связь пациент ↔ палата — по паре (Department, WardNumber), а НЕ по WardId.
    /// </summary>
    public class WardService
    {
        private readonly ApplicationContext _context;

        public WardService(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Отделения, участвующие в размещении по палатам, в порядке сводки.
        /// Отбор — только по флагу ShowOnWards; обязательный Include, иначе
        /// навигация Settings молча пустая и список оказывается пустым.
        /// </summary>
        public List<Department> GetWardDepartments()
        {
            return _context.Departments
                .AsNoTracking()
                .Include(d => d.Settings)
                .Where(d => d.Settings != null && d.Settings.ShowOnWards)
                .OrderBy(d => d.ShowOrder)
                .ToList();
        }

        /// <summary>Палаты отделения в порядке показа (числовые по возрастанию, затем именованные по алфавиту).</summary>
        public List<Ward> GetWardsByDepartment(int departmentId)
        {
            var wards = _context.Wards
                .AsNoTracking()
                .Where(w => w.Department == departmentId)
                .ToList();

            wards.Sort(WardOrder);
            return wards;
        }

        public WardBoardData GetWardBoard()
        {
            var departments = GetWardDepartments();
            if (departments.Count == 0)
            {
                return new WardBoardData();
            }

            var departmentIds = departments.Select(d => d.Id).ToList();

            var wards = _context.Wards
                .AsNoTracking()
                .Where(w => departmentIds.Contains(w.Department))
                .ToList();

            var patients = _context.Patients
                .AsNoTracking()
                .Where(p => departmentIds.Contains(p.Department))
                .OrderBy(p => p.Id)
                .ToList();

            var patientsByWard = patients
                .GroupBy(p => WardKey(p.Department, p.WardNumber))
                .ToDictionary(g => g.Key, g => g.ToList());

            var board = new WardBoardData();

            foreach (var department in departments)
            {
                var departmentWards = wards
                    .Where(w => w.Department == department.Id)
                    .ToList();

                departmentWards.Sort(WardOrder);

                board.Departments.Add(new WardBoardDepartment
                {
                    Id = department.Id,
                    Name = department.Name ?? string.Empty,
                    Wards = departmentWards.Select(ward =>
                    {
                        patientsByWard.TryGetValue(WardKey(ward.Department, ward.Number), out var wardPatients);

                        return new WardBoardWard
                        {
                            Number = ward.Number,
                            Capacity = ward.Capacity,
                            CanPut = ward.CanPut,
                            Patients = (wardPatients ?? new List<Patient>())
                                .Select(patient => new WardBoardPatient
                                {
                                    Male = patient.Male,
                                    AgeText = PatientAge.Format(patient),
                                    MkbCode = patient.MkbCode ?? string.Empty,
                                    HospitalisationDate = patient.HospitalisationDate
                                })
                                .ToList()
                        };
                    }).ToList()
                });
            }

            return board;
        }

        private static string WardKey(int departmentId, string? wardNumber) =>
            $"{departmentId}\u001f{(wardNumber ?? string.Empty).Trim().ToUpperInvariant()}";

        public Ward? GetWard(int id)
        {
            return _context.Wards.AsNoTracking().FirstOrDefault(w => w.Id == id);
        }

        /// <summary>
        /// Пациенты палаты. Сравнение номера идёт в памяти: SQLite не приводит
        /// регистр кириллицы, а номера палат бывают именованными.
        /// </summary>
        public List<Patient> GetPatientsInWard(int departmentId, string wardNumber)
        {
            var number = (wardNumber ?? string.Empty).Trim();

            var patients = _context.Patients
                .AsNoTracking()
                .Where(p => p.Department == departmentId)
                .ToList();

            return patients
                .Where(p => string.Equals((p.WardNumber ?? string.Empty).Trim(), number, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public bool Add(int departmentId, string number, int capacity, out string error)
        {
            error = string.Empty;
            number = (number ?? string.Empty).Trim();

            if (!ValidateNumberAndCapacity(number, capacity, out error))
            {
                return false;
            }

            if (NumberExists(departmentId, number))
            {
                error = $"Палата с номером «{number}» в этом отделении уже есть.";
                return false;
            }

            _context.Wards.Add(new Ward
            {
                Number = number,
                Department = departmentId,
                Capacity = capacity,
                CanPut = true
            });
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Правка палаты: номер и вместимость. Переименование каскадно переносит
        /// пациентов палаты на новый номер — в одной транзакции с самим переименованием.
        /// </summary>
        public bool Update(Ward ward, out string error)
        {
            error = string.Empty;

            if (ward.Id == 0)
            {
                error = "Не выбрана палата.";
                return false;
            }

            var number = (ward.Number ?? string.Empty).Trim();
            if (!ValidateNumberAndCapacity(number, ward.Capacity, out error))
            {
                return false;
            }

            var stored = _context.Wards.FirstOrDefault(w => w.Id == ward.Id);
            if (stored is null)
            {
                error = "Палата не найдена.";
                return false;
            }

            if (NumberExists(stored.Department, number, ward.Id))
            {
                error = $"Палата с номером «{number}» в этом отделении уже есть.";
                return false;
            }

            var oldNumber = stored.Number ?? string.Empty;

            using var transaction = _context.Database.BeginTransaction();

            stored.Number = number;
            stored.Capacity = ward.Capacity;

            if (!string.Equals(oldNumber.Trim(), number, StringComparison.OrdinalIgnoreCase))
            {
                var patients = _context.Patients
                    .Where(p => p.Department == stored.Department)
                    .ToList();

                foreach (var patient in patients)
                {
                    if (string.Equals((patient.WardNumber ?? string.Empty).Trim(), oldNumber.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        patient.WardNumber = number;
                    }
                }
            }

            _context.SaveChanges();
            transaction.Commit();
            return true;
        }

        /// <summary>
        /// Переключение признака «закрыта для пациентов» (страница «Места»). Номер и
        /// вместимость здесь не меняются — они правятся только в «Администрировании (палаты)».
        /// </summary>
        public bool SetCanPut(int wardId, bool canPut, out string error)
        {
            error = string.Empty;

            var stored = _context.Wards.FirstOrDefault(w => w.Id == wardId);
            if (stored is null)
            {
                error = "Палата не найдена.";
                return false;
            }

            stored.CanPut = canPut;
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Удаление палаты. Запрещено, пока в палате есть пациенты: пациенты
        /// при удалении палаты не удаляются никогда.
        /// </summary>
        public bool Delete(int id, out string error)
        {
            error = string.Empty;

            var stored = _context.Wards.FirstOrDefault(w => w.Id == id);
            if (stored is null)
            {
                error = "Палата не найдена.";
                return false;
            }

            if (GetPatientsInWard(stored.Department, stored.Number).Count > 0)
            {
                error = $"Палату «{stored.Number}» нельзя удалить: в ней есть пациенты.";
                return false;
            }

            _context.Wards.Remove(stored);
            _context.SaveChanges();
            return true;
        }

        /// <summary>Есть ли в отделении другая палата с таким номером (без учёта регистра и пробелов).</summary>
        private bool NumberExists(int departmentId, string number, int exceptId = 0)
        {
            var numbers = _context.Wards
                .AsNoTracking()
                .Where(w => w.Id != exceptId && w.Department == departmentId)
                .Select(w => w.Number)
                .ToList();

            return numbers.Any(n => string.Equals((n ?? string.Empty).Trim(), number, StringComparison.OrdinalIgnoreCase));
        }

        private static bool ValidateNumberAndCapacity(string number, int capacity, out string error)
        {
            if (string.IsNullOrEmpty(number))
            {
                error = "Укажите номер палаты.";
                return false;
            }

            if (capacity < 0)
            {
                error = "Вместимость не может быть отрицательной.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static readonly IComparer<Ward> WardOrder = Comparer<Ward>.Create((left, right) =>
        {
            var leftNumeric = int.TryParse(left.Number, NumberStyles.Integer, CultureInfo.InvariantCulture, out var leftValue);
            var rightNumeric = int.TryParse(right.Number, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rightValue);

            if (leftNumeric && rightNumeric)
            {
                return leftValue.CompareTo(rightValue);
            }

            if (leftNumeric)
            {
                return -1;
            }

            if (rightNumeric)
            {
                return 1;
            }

            return string.Compare(left.Number ?? string.Empty, right.Number ?? string.Empty, StringComparison.CurrentCulture);
        });
    }
}
