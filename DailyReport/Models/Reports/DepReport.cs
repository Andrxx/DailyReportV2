using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyReport.Models.Reports
{
    public class DepReport : ICloneable
    {
        public List<ReportLine>? Lines { get; set; }

        internal int incomeChildren;
        public int Id { get; set; }

        /// <summary>
        /// Фактическое время сохранения сводки. Служебное: на принадлежность к суткам
        /// больше не влияет, использоваться может для аудита.
        /// </summary>
        public DateTime date { get; set; } = DateTime.Now;//.AddDays(-1);

        /// <summary>
        /// Отчётные сутки сводки, то есть чистая дата без времени.
        /// Вместе с depNumber образует ключ: одна сводка на отделение в сутки.
        /// Задаётся формой при создании и дальше не меняется.
        /// null бывает только у нерассчитанных служебных строк, их в SQLite
        /// уникальный индекс допускает сколько угодно.
        /// </summary>
        public DateOnly? reportDate { get; set; }

        //public int? ShowOrder { get; set; }
        public int depNumber { get; set; }
        public int existed { get; set; }
        public int existedChildren { get; set; }
        public int income { get; set; }
        public int incomeChildrens { get; set; }
        public int outcome { get; set; }
        public int outcomeChildrens { get; set; }
        public int movedOutDep { get; set; }
        public int movedOutDepChildrens { get; set; }
        public int movedInDep { get; set; }
        public int movedInDepChildrens { get; set; }
        public int died { get; set; }
        public int diedChildrens { get; set; }
        public int present { get; set; }
        public int presentChildrens { get; set; }
        public int attachedToORIT { get; set; }
        public int attachedToORITCildrens { get; set; }
        public int oIVL { get; set; }
        public int oIVLChildrens { get; set; }
        public int oNIVL { get; set; }
        public int oNIVLChildrens { get; set; }
        public int oNIVLVPO { get; set; }
        public int oNIVLVPOChildrens { get; set; }
        public int oNIVLMask { get; set; }
        public int oNIVLMaskChildrens { get; set; }
        public int oMask { get; set; }
        public int oMaskChildren { get; set; }
        public int oBrease { get; set; }
        public int oBreaseChildrens { get; set; }
        public int pregnant { get; set; }
        public int pregnantChildrens { get; set; }
        public int restZone { get; set; }
        public int restZoneChildrens { get; set; }
        public int outRegions { get; set; }
        public int outRegionsChildrens { get; set; }
        public int forein { get; set; }
        public int foreinChildrens { get; set; }
        public int LNR_DNR { get; set; }
        public int LNR_DNRChildrens { get; set; }
        public int otherUkrane { get; set; }
        public int otherUkraneChildren { get; set; }
        public int incomeHospital { get; set; }
        public int incomeHospitalChildrens { get; set; }
        public int outcomeHospital { get; set; }
        public int outcomeHospitalChildrens { get; set; }
        public int U071 { get; set; }
        public int U071Childrens { get; set; }
        public int U072 { get; set; }
        public int U072Childrens { get; set; }
        public int ORVI { get; set; }
        public int ORVIChildrens { get; set; }
        public int grippe { get; set; }
        public int grippeChildrens { get; set; }
        public int pneumonia { get; set; }
        public int pneumoniaChildrens { get; set; }
        public int OKI { get; set; }
        public int OKIChildrens { get; set; }
        public int meningit { get; set; }
        public int meningitChildrens { get; set; }
        public int hepatit { get; set; }
        public int hepatitChildrens { get; set; }
        public int HIV { get; set; }
        public int HIVCildrens { get; set; }
        public int other { get; set; }
        public int otherChildrens { get; set; }
        public int sepsis { get; set; }
        public int sepsisChildren { get; set; }
        public int measles { get; set; }
        public int measlesChildren { get; set; }

        //уход
        public int care { get; set; }
        public int careDisodered { get; set; }
        public int presentWithCare { get; set; }
        public int presentWithCareChildren { get; set; }

        //public List<LineEntity>? lines { get; set; }
        public string? dutyNurse { get; set; }

        //строки с дополнительными данными
        public int optionalSocial { get; set; }
        public int optionalSocialChildren { get; set; }
        public int optionalNozology { get; set; }
        public int optionalNozologyChildren { get; set; }
        public int optionalOxygen { get; set; }
        public int optionalOxygenChildren { get; set; }
        public int optionalCare { get; set; }
        public int optionalCarelChildren { get; set; }


        /// <summary>
        /// подсчет количества больных на кислороде (считаем и детей и взрослых)
        /// </summary>
        /// <returns></returns>
        public int CountO2()
        {
            int _summary = oIVL + oMask + oNIVL + oNIVLMask + oNIVLVPO + oIVLChildrens + oMaskChildren + oNIVLChildrens
                + oNIVLMaskChildrens + oNIVLVPOChildrens + optionalOxygen + optionalOxygenChildren;
            return _summary;
        }
        /// <summary>
        /// Подсчет количества взрослых больных
        /// </summary>
        /// <returns></returns>
        public int CountDiseases()
        {
            int _summary = U071 + U072 + ORVI + pneumonia + OKI + grippe + meningit + hepatit
                + HIV + other + sepsis + measles + optionalNozology;
            return _summary;
        }
        /// <summary>
        /// Подсчет количества больных детей
        /// </summary>
        /// <returns></returns>
        public int CountDiseasesChildren()
        {
            int _summary = U071Childrens + U072Childrens + ORVIChildrens + grippeChildrens + pneumoniaChildrens + OKIChildrens + meningitChildrens +
                hepatitChildrens + HIVCildrens + otherChildrens + sepsisChildren + measlesChildren + optionalNozologyChildren;
            return _summary;
        }
        /// <summary>
        /// Считаем всех с учетом ухода. Запускать после заполнения всех полей
        /// </summary>
        /// <returns></returns>
        public int CountPresentCare()
        {
            int _summary = present + care;
            return _summary;
        }

        /// <summary>
        /// Считаем взрослых из Украины
        /// </summary>
        /// <returns></returns>
        public int CountUcrane()
        {
            return LNR_DNR + otherUkrane;
        }
        /// <summary>
        /// Считаем детей из Украины
        /// </summary>
        /// <returns></returns>
        public int CountUkraneChildren()
        {
            return LNR_DNRChildrens + otherUkraneChildren;
        }


        public object Clone()
        {
            return MemberwiseClone();
        }
    }


}
