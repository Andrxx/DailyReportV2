using System.Globalization;
using DailyReport.Models;
using DailyReport.Models.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DailyReport.Services.Reports
{
    public sealed record AdmissionDayPoint(DateOnly Date, string Label, int? Value);

    public sealed record NozologySlice(string Label, int Value, int ColorIndex);

    public sealed record AdmissionStats(
        DateOnly AsOf,
        IReadOnlyList<AdmissionDayPoint> Month,
        IReadOnlyList<AdmissionDayPoint> Week,
        IReadOnlyList<NozologySlice> Nozologies);

    public class AdmissionStatsService
    {
        private readonly ApplicationContext _context;
        private readonly SummaryService _summaryService;
        private readonly IConfiguration _configuration;

        public AdmissionStatsService(
            ApplicationContext context,
            SummaryService summaryService,
            IConfiguration configuration)
        {
            _context = context;
            _summaryService = summaryService;
            _configuration = configuration;
        }

        public AdmissionStats Build(DateOnly asOf)
        {
            var monthStart = new DateOnly(asOf.Year, asOf.Month, 1);
            var weekStart = asOf.AddDays(-6);
            var from = monthStart < weekStart ? monthStart : weekStart;

            var allowedIds = _context.Departments
                .AsNoTracking()
                .Where(d => d.Settings != null && d.Settings.ShowOnFinalReport)
                .Select(d => d.Id)
                .ToHashSet();

            var reports = _context.DepReports
                .AsNoTracking()
                .Where(r => r.reportDate >= from && r.reportDate <= asOf)
                .Select(r => new
                {
                    r.depNumber,
                    r.reportDate,
                    r.income,
                    r.incomeChildrens
                })
                .ToList()
                .Where(r => allowedIds.Contains(r.depNumber) && r.reportDate.HasValue)
                .GroupBy(r => (r.depNumber, r.reportDate!.Value))
                .Select(g => g.First())
                .ToList();

            var byDay = reports
                .GroupBy(r => r.reportDate!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r => r.income + r.incomeChildrens));

            var month = new List<AdmissionDayPoint>();
            var daysInMonth = DateTime.DaysInMonth(asOf.Year, asOf.Month);

            for (var day = 1; day <= daysInMonth; day++)
            {
                var date = new DateOnly(asOf.Year, asOf.Month, day);
                int? value = date > asOf ? null : byDay.GetValueOrDefault(date);
                month.Add(new AdmissionDayPoint(date, date.ToString("dd.MM", CultureInfo.InvariantCulture), value));
            }

            var week = new List<AdmissionDayPoint>();
            for (var offset = -6; offset <= 0; offset++)
            {
                var date = asOf.AddDays(offset);
                week.Add(new AdmissionDayPoint(
                    date,
                    date.ToString("dd.MM", CultureInfo.InvariantCulture),
                    byDay.GetValueOrDefault(date)));
            }

            var config = SummaryConfigFactory.Create(_configuration);
            var summary = _summaryService.Build(asOf, config);

            var nozologies = summary.Rows
                .Where(r => r.Row.Section == SummarySection.Nozology && !r.Row.IsSubtotal)
                .Select((r, index) => new NozologySlice(
                    r.Row.Label,
                    r.AdultsTotal + r.ChildrenTotal,
                    index))
                .Where(s => s.Value > 0 && !string.IsNullOrWhiteSpace(s.Label))
                .OrderByDescending(s => s.Value)
                .ThenBy(s => s.Label, StringComparer.CurrentCulture)
                .ToList();

            return new AdmissionStats(asOf, month, week, nozologies);
        }
    }
}
