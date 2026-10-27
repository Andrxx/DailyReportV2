using DailyReport.Models.Reports;
using Microsoft.Extensions.Configuration;

namespace DailyReport.Services.Reports
{
    public static class SummaryConfigFactory
    {
        public static IReadOnlyDictionary<SummaryConfigGroup, SummaryRowConfig> Create(IConfiguration configuration)
        {
            return new Dictionary<SummaryConfigGroup, SummaryRowConfig>
            {
                [SummaryConfigGroup.Oxygen] = Read(configuration, "Oxygen"),
                [SummaryConfigGroup.Social] = Read(configuration, "Social"),
                [SummaryConfigGroup.Nozology] = Read(configuration, "Nozology"),
                [SummaryConfigGroup.Care] = Read(configuration, "Care")
            };
        }

        private static SummaryRowConfig Read(IConfiguration configuration, string group)
        {
            var name = configuration[$"{group}:Name"];
            var visible = bool.TryParse(configuration[$"{group}:Visible"], out var result) && result;
            return new SummaryRowConfig(name ?? string.Empty, visible);
        }
    }
}
