using DailyReport.Models;
using DailyReport.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace DailyReport.Services.Reports
{
    /// <summary>
    /// Подпись и видимость одной опциональной строки сводки (из конфигурации приложения).
    /// </summary>
    public sealed record SummaryRowConfig(string Label, bool Visible);

    /// <summary>
    /// Сборка итоговой сводки за отчётные сутки. Только чтение: страница ничего не пишет в БД.
    /// </summary>
    public class SummaryService
    {
        private readonly ApplicationContext _context;

        public SummaryService(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Собирает сводку за отчётные сутки.
        /// </summary>
        public FinalReportData Build(
            DateOnly reportDate,
            IReadOnlyDictionary<SummaryConfigGroup, SummaryRowConfig> config)
        {
            // Settings обязателен: по нему определяются роль отделения, попадание
            // в колонки и участие в коечном фонде. Без Include навигация не грузится
            // и все проверки молча дают null.
            var departments = (from d in _context.Departments
                               where d.Settings != null && d.Settings.ShowOnFinalReport
                               orderby d.ShowOrder
                               select d).Include(d => d.Settings).ToList();

            var reports = _context.DepReports
                .AsNoTracking()
                .Where(r => r.reportDate == reportDate)
                .ToList();

            var byNumber = reports
                .GroupBy(r => r.depNumber)
                .ToDictionary(g => g.Key, g => g.First());

            var columns = new List<SummaryColumn>();
            foreach (var dep in departments)
            {
                byNumber.TryGetValue(dep.Id, out var report);
                columns.Add(new SummaryColumn
                {
                    Department = dep,
                    Report = report ?? new DepReport { depNumber = dep.Id, reportDate = reportDate },
                    HasData = report is not null
                });
            }

            var dayCareColumn = columns.FirstOrDefault(c => c.IsDayCare);

            var rows = new List<SummaryRowValues>();
            foreach (var row in SummaryRowCatalog.Rows())
            {
                if (row.ConfigGroup != SummaryConfigGroup.None)
                {
                    if (config.TryGetValue(row.ConfigGroup, out var rowConfig) && !rowConfig.Visible)
                    {
                        continue;
                    }
                    row.Label = config.TryGetValue(row.ConfigGroup, out var named) ? named.Label : string.Empty;
                }

                rows.Add(Resolve(row, columns, dayCareColumn));
            }

            var counted = columns.Where(c => c.Department.Settings?.CountSpotsInFinalReport == true).ToList();

            // Коечный фонд для строки «Фактическое количество коек». Первый блок итогов —
            // все отделения сводки, кроме дневного стационара, второй добавляет его.
            var inpatient = columns.Where(c => !c.IsDayCare).ToList();

            return new FinalReportData
            {
                ReportDate = reportDate,
                Columns = columns,
                Rows = rows,
                AdultSpots = counted.Sum(c => c.Department.AdultSpotsQuantity),
                ChildrenSpots = counted.Sum(c => c.Department.ChildrenSpotsQuantity),
                BedsAdults = inpatient.Sum(c => c.AdultSpots),
                BedsChildren = inpatient.Sum(c => c.ChildrenSpots),
                DayCareBedsAdults = columns.Where(c => c.IsDayCare).Sum(c => c.AdultSpots),
                DayCareBedsChildren = columns.Where(c => c.IsDayCare).Sum(c => c.ChildrenSpots),
                OritSpots = columns
                    .Where(c => c.Department.Settings?.Type.IsOrit() == true)
                    .Sum(c => c.Spots),
                KdoVisits = FindVisits(DepartmentType.KDO, byNumber),
                PayServiceVisits = FindVisits(DepartmentType.PayService, byNumber)
            };
        }

        /// <summary>
        /// Раскладывает показатель по колонкам. Дневной стационар идёт отдельной
        /// колонкой в таблице, но в сумму стационара не входит.
        /// </summary>
        private static SummaryRowValues Resolve(
            SummaryRow row,
            List<SummaryColumn> columns,
            SummaryColumn? dayCare)
        {
            var adults = new int[columns.Count];
            var children = new int[columns.Count];
            var merged = new int[columns.Count];

            int dayCareAdults = 0;
            int dayCareChildren = 0;
            int dayCareMerged = 0;

            // Дневной стационар ведёт только движение пациентов. Уход, нозологии,
            // кислород и прочие строки он не собирает, поэтому подмешивать его туда
            // нельзя: «лиц по уходу» в дневном стационаре не бывает.
            // Это правило относится только к ИТОГАМ (countsDayCare ниже): сама
            // колонка отделения показывает значения из отчёта дневного стационара
            // во всех разделах, как в V1 (FinalReport.cshtml, @dr.present + @dr.care).
            bool countsDayCare = row.CountsInDayCareTotal;

            for (int i = 0; i < columns.Count; i++)
            {
                var report = columns[i].Report;
                adults[i] = row.Adults?.Invoke(report) ?? 0;
                children[i] = row.Children?.Invoke(report) ?? 0;
                merged[i] = row.Merged?.Invoke(report) ?? 0;
            }

            if (countsDayCare)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    if (dayCare is not null && ReferenceEquals(columns[i], dayCare))
                    {
                        dayCareAdults = adults[i];
                        dayCareChildren = children[i];
                        dayCareMerged = merged[i];
                    }
                }
            }

            var countedIndexes = Enumerable.Range(0, columns.Count)
                .Where(i => dayCare is null || !ReferenceEquals(columns[i], dayCare));

            return new SummaryRowValues
            {
                Row = row,
                Adults = adults,
                Children = children,
                Merged = merged,
                AdultsTotal = countedIndexes.Sum(i => adults[i]),
                ChildrenTotal = countedIndexes.Sum(i => children[i]),
                MergedTotal = countedIndexes.Sum(i => merged[i]),
                DayCareAdults = dayCareAdults,
                DayCareChildren = dayCareChildren,
                DayCareMerged = dayCareMerged
            };
        }

        /// <summary>
        /// Обратившиеся по упрощённой форме отделения: у КДО и платных услуг
        /// заполняется только «Обратилось».
        /// </summary>
        private int FindVisits(DepartmentType type, Dictionary<int, DepReport> byNumber)
        {
            var ids = _context.Departments
                .AsNoTracking()
                .Where(d => d.Settings != null && d.Settings.Type == type)
                .Select(d => d.Id)
                .ToList();

            return ids.Where(byNumber.ContainsKey).Sum(id => byNumber[id].present);
        }
    }
}
