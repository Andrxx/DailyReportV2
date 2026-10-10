using System.ComponentModel.DataAnnotations.Schema;

namespace DailyReport.Models.WardsModels
{
    public class Ward
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int Department { get; set; }
        public int Capacity { get; set; }
        //public List<int>? PatientsId { get; set; }
        public bool CanPut { get; set; } = true;
        public bool IsDirtyZone { get; set; } = false;
        [NotMapped]
        public List<Patient> PatientsInWard { get; set; } = new();

    }
}
