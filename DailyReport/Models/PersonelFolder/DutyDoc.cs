namespace DailyReport.Models.PersonelFolder
{
    public class DutyDoc
    {
        public int Id { get; set; }
        public string? dutyDoc { get; set; }
        public string? departments { get; set; }

        /// <summary>
        /// Отчётные сутки дежурства. Задаётся формой при создании и не пересчитывается:
        /// dutyDate ниже — только факт сохранения, по нему сутки определять нельзя.
        /// </summary>
        public DateOnly? reportDate { get; set; }

        public DateTime dutyDate { get; set; } //= DateTime.Now.Date;//.AddDays(-1);
        public DutyType type { get; set; }
    }

    public enum DutyType
    {
        Department,
        Reanimanion,
        Rentgenology
    }
}
