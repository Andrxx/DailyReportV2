using System.ComponentModel.DataAnnotations.Schema;

namespace DailyReport.Models.WardsModels
{
    public class Patient
    {
        public int Id { get; set; }
        public int Department { get; set; }
        public string WardNumber { get; set; }
        public string Name { get; set; }
        public string Male { get; set; }
        public int Age { get; set; }
        public int? AgeMonts { get; set; }
        public string? sAge { get; set; }
        public string Diagnos { get; set; }
        public DateTime HospitalisationDate { get; set; }
        public bool HasRash { get; set; }
        public bool IsUntochable { get; set; }
        public bool IsDisodered { get; set; }
        public bool HasCareRisk { get; set; }

    }

    public class WardTransferData
    {
        public string? WardNumber { get; set; }
        public int PatientId { get; set; }
    }


    //public class ShortPatient
    //{
    //    public int PatientId { get; set; }
    //    public int Department { get; set; }
    //    public int WardId { get; set; }
    //    [ForeignKey("WardId")]
    //    public virtual ShortWard? Ward { get; set; }
    //    public string Gender { get; set; }
    //    public string Diagnos { get; set; }
    //    public string? HospitalisationDate { get; set; }
    //    public int Age { get; set; }
    //    public int? AgeMonth { get; set; }
    //}
}
