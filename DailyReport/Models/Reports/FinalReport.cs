using System.ComponentModel.DataAnnotations.Schema;

namespace DailyReport.Models.Reports
{
    public class FinalReport
    {
        public int Id { get; set; }
        public DateTime date { get; set; } = DateTime.Now.Date.AddDays(-1);
        public int existed { get; set; }
        public int existedChildren { get; set; }
        public int income { get; set; }
        public int incomeChildren { get; set; }
        public int outcome { get; set; }
        public int outcomeChildren { get; set; }
        public int attachedToORIT { get; set; }
        public int attachedToORITChildren { get; set; }
        public int movedOutDep { get; set; }
        public int movedOutDepChildren { get; set; }
        public int movedInDep { get; set; }
        public int movedInDepChildren { get; set; }
        public int died { get; set; }
        public int diedChildren { get; set; }
        public int present { get; set; }
        public int presentChildren { get; set; }
        public int oIVL { get; set; }
        public int oIVLChildren { get; set; }
        public int oNIVL { get; set; }
        public int oNIVLChildren { get; set; }
        public int oNIVLVPO { get; set; }
        public int oNIVLVPOChildren { get; set; }
        public int oNIVLMask { get; set; }
        public int oNIVLMaskChildren { get; set; }
        public int oMask { get; set; }
        public int oMaskChildren { get; set; }
        public int oBrease { get; set; }
        public int oBreaseChildren { get; set; }
        public int pregnant { get; set; }
        public int pregnantChildren { get; set; }
        public int restZone { get; set; }
        public int restZoneChildren { get; set; }
        public int outRegions { get; set; }
        public int outRegionsChildren { get; set; }
        public int forein { get; set; }
        public int foreinChildren { get; set; }
        public int LNR_DNR { get; set; }
        public int LNR_DNRChildren { get; set; }
        public int otherUkrane { get; set; }
        public int otherUkraneChildren { get; set; }
        public int incomeHospital { get; set; }
        public int incomeHospitalChildren { get; set; }
        public int outcomeHospital { get; set; }
        public int outcomeHospitalChildren { get; set; }
        public int U071 { get; set; }
        public int U071Children { get; set; }
        public int U072 { get; set; }
        public int U072Children { get; set; }
        public int ORVI { get; set; }
        public int ORVIChildren { get; set; }
        public int grippe { get; set; }
        public int grippeChildren { get; set; }
        public int pneumonia { get; set; }
        public int pneumoniaChildren { get; set; }
        public int OKI { get; set; }
        public int OKIChildren { get; set; }
        public int meningit { get; set; }
        public int meningitChildren { get; set; }
        public int sepsis { get; set; }
        public int sepsisChildren { get; set; }
        public int hepatit { get; set; }
        public int hepatitChildren { get; set; }
        public int HIV { get; set; }
        public int HIVCildren { get; set; }
        public int other { get; set; }
        public int otherChildren { get; set; }
        public int oSummary { get; set; }
        public int oSummaryChildren { get; set; }
        public int nozSummary { get; set; }
        public int nozSummaryChildren { get; set; }
        public int presentNonDaycare { get; set; }
        public int presentNonDaycareChildren { get; set; }
        public int measles { get; set; }
        public int measlesChildren { get; set; }

        //уход
        public int care { get; set; }
        public int careDisodered { get; set; }
        public int presentWithCare { get; set; }
        public int presentWithCareChildren { get; set; }

        //дополнительные поля сводки
        public int optionalSocial { get; set; }
        public int optionalSocialChildren { get; set; }
        public int optionalNozology { get; set; }
        public int optionalNozologyChildren { get; set; }
        public int optionalOxygen { get; set; }
        public int optionalOxygenChildren { get; set; }
        public int optionalCare { get; set; }
        public int optionalCarelChildren { get; set; }

        //не госпитализированые пациенты
        public int reject { get; set; }
        public int rejectChildren { get; set; }
        public int ambulance { get; set; }
        public int ambulanceChildren { get; set; }
        public int sendToMO { get; set; }
        public int sendToMOChildren { get; set; }
        public int sumAdults { get; set; }
        public int sumChild { get; set; }
        public int sumAll { get; set; }

        //public int IFSO { get; set; }               //участники олимпиады по фин. безопасности
        //public int IFSOChildren { get; set; }       //поле временно введено в связи с событием

        /// <summary>
        /// Считаем больных по отделениям
        /// </summary>
        /// <returns></returns>
        public int CountDiseases()
        {
            int _summary = U071 + U072 + ORVI + grippe + pneumonia + OKI + meningit + hepatit + HIV + other + sepsis + measles + optionalNozology;
            return _summary;
        }
        /// <summary>
        /// Считаем больных детей по отделениям
        /// </summary>
        /// <returns></returns>
        public int CountDiseasesChildren()
        {
            int _summary = U071Children + U072Children + ORVIChildren + grippeChildren + pneumoniaChildren + OKIChildren + meningitChildren +
                hepatitChildren + HIVCildren + otherChildren + sepsisChildren + measlesChildren + optionalNozologyChildren;
            return _summary;
        }

    }
}
