using System.ComponentModel.DataAnnotations.Schema;

namespace DailyReport.Models.Reports
{
    public class ReportLine
    {
        public int id { get; set; }
        public int DepartmentId { get; set; } 
        public string? name { get; set; }
        public string? lineType { get; set; }
        //public DateTime reportDate { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
    }
}
