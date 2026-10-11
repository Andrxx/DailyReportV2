using System.ComponentModel.DataAnnotations.Schema;

namespace DailyReport.Models.Reports
{
    public class FireReport
    {
        public int Id { get; set; }
        public int DepNumber { get; set; }
        public int Adult { get; set; }
        public int Children { get; set; }
        public int Care { get; set; }
        /// <summary>
        /// Нетранспортабельные взрослые. Это ПОДМНОЖЕСТВО пациентов (уже входит в Adult),
        /// поэтому в итоги «Всего взр/дет/человек» НЕ добавляется — только показывается.
        /// </summary>
        public int NonTransportableAdults { get; set; }
        /// <summary>
        /// Нетранспортабельные дети. Это ПОДМНОЖЕСТВО пациентов (уже входит в Children),
        /// поэтому в итоги «Всего взр/дет/человек» НЕ добавляется — только показывается.
        /// </summary>
        public int NonTransportableChildren { get; set; }
        public int Personel { get; set; }
        public DateTime Date { get; set; }
        /// <summary>
        /// Отчётные сутки пожарной сводки (чистая дата). Граница суток у пожарной
        /// сводки своя — 08:00, она не совпадает с границей сводки отделений (07:30).
        /// Date остаётся только отметкой о фактическом времени сохранения.
        /// </summary>
        public DateOnly? ReportDate { get; set; }
        public int? ShowOrder { get; set; }
        [NotMapped]
        public bool IsAdded { get; set; }
        public int Doctors { get; set; }      // Врачи
        public int Nurses { get; set; }       // м/с (медицинские сёстры)
        public int Sanitarians { get; set; }  // сан (санитары)

        public string ToDepName()
        {
            string name;
            switch (DepNumber)
            {
                case 1:
                    {
                        name = "Первое отделение";
                        return name;
                    }
                case 11:
                    {
                        name = "Первое отделение грязная зона";
                        return name;
                    }
                case 2:
                    {
                        name = "Второе отделение";
                        return name;
                    }
                case 3:
                    {
                        name = "Третье отделение";
                        return name;
                    }
                case 4:
                    {
                        name = "Четвертое отделение";
                        return name;
                    }
                case 5:
                    {
                        name = "Пятое отделение";
                        return name;
                    }
                case 6:
                    {
                        name = "Шестое отделение";
                        return name;
                    }
                case 7:
                    {
                        name = "Седьмое отделение";
                        return name;
                    }
                case 8:
                    {
                        name = "Восьмое отделение";
                        return name;
                    }
                case 21:
                    {
                        name = "Дневной стационар";
                        return name;
                    }
                case 90:
                    {
                        name = "ОРИТ";
                        return name;
                    }
                case 91:
                    {
                        name = "ОРИТ грязная зона";
                        return name;
                    }
                case 25:
                    {
                        name = "Лаборатория";
                        return name;
                    }
                case 102:
                    {
                        name = "Охрана и тех.персонал";
                        return name;
                    }
                default:
                    {
                        name = "";
                        return name;
                    }
            }
        }
    }
}
