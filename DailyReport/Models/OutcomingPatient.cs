namespace DailyReport.Models
{
    /// <summary>
    /// Пациент, отказавшийся от госпитализации, и его маршрут — блок «Переводы и отказы»
    /// внизу итоговой сводки (перенос из V1).
    ///
    /// Отличие от V1: сутки определяются полем ReportDate, а не окном времени по Date.
    /// В V1 отбиралось окном 07:00 → 07:00 следующего дня, и запись ровно на границе
    /// в него не попадала. Здесь ReportDate — чистая дата, выборка идёт строго по
    /// равенству, а Date остаётся только фактом сохранения.
    /// </summary>
    public class OutcomingPatient
    {
        public int Id { get; set; }

        /// <summary>ФИО пациента</summary>
        public string? Name { get; set; }

        /// <summary>Пол: «М» или «Ж»</summary>
        public string? Gender { get; set; }

        /// <summary>Возраст в годах. Строка, а не число: в V1 сюда попадали значения вроде «3.5».</summary>
        public string? AgeYears { get; set; }

        /// <summary>Возраст в месяцах — для детей до года.</summary>
        public string? AgeMonth { get; set; }

        /// <summary>Кем доставлен</summary>
        public string? Shipped { get; set; }

        /// <summary>Диагноз</summary>
        public string? Diagnos { get; set; }

        /// <summary>Откуда направлен</summary>
        public string? SubmitedFrom { get; set; }

        /// <summary>Куда переведён или направлен: больница, поликлиника, и т. п. Свободный текст.</summary>
        public string? SubmitedTo { get; set; }

        /// <summary>
        /// Тип маршрута — по нему считаются строки «Отказ от госпитализации»,
        /// «На амбулаторное лечение» и «В другую МО» в таблице сводки.
        ///
        /// В V1 тип определялся сравнением строки SubmitedTo с литералом
        /// «отказался»/«амбулаторно»: в базе лежали «Отказ», «отказ», «АМБУЛАТОРНО»,
        /// и 13 записей об отказе уезжали в «в другую МО». Здесь тип выбирается явно,
        /// а если он не задан (старые записи) — определяется по тексту, см.
        /// <see cref="Services.Reports.OutPatientService.RouteFromText"/>.
        /// </summary>
        public OutRoute? Route { get; set; }

        /// <summary>
        /// Отчётные сутки. Задаются формой при создании и не пересчитываются:
        /// Date ниже — только факт сохранения, по нему сутки определять нельзя.
        /// </summary>
        public DateOnly? ReportDate { get; set; }

        public DateTime Date { get; set; }
    }

    /// <summary>Куда в итоге пошёл отказавшийся от госпитализации пациент.</summary>
    public enum OutRoute
    {
        /// <summary>Отказ от госпитализации — больной не принят.</summary>
        Reject,

        /// <summary>На амбулаторное лечение.</summary>
        Ambulatory,

        /// <summary>Направлен в другую медицинскую организацию.</summary>
        OtherHospital
    }
}