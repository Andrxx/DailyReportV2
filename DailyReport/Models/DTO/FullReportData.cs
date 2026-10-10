using DailyReport.Models.Reports;

namespace DailyReport.Models.DTO
{
    public class FullReportData
    {
        public DepReport? Report { get; set; }
        public Department? Department { get; set; }
        public bool ShowOnMain { get; set; }
        public bool ShowInFinalReport { get; set; }
        public bool CountSpots { get; set; }

        //"FinalReportORITSpotsCorrection": "90 91",
    }
}
