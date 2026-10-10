using DailyReport.Models.Reports;

namespace DailyReport.Services.Reports
{
    /// <summary>
    /// Каталог показателей итоговой сводки. Состав строк фиксирован в коде и совпадает
    /// с формой сводки отделения; подписи четырёх опциональных строк приходят из конфигурации.
    /// </summary>
    public static class SummaryRowCatalog
    {
        private static readonly SummarySection[] sectionOrder =
        {
            SummarySection.Movement,
            SummarySection.Oxygen,
            SummarySection.SocialStatus,
            SummarySection.Nozology,
            SummarySection.Care
        };

        public static IReadOnlyList<SummarySection> Sections => sectionOrder;

        public static string SectionTitle(SummarySection section) => section switch
        {
            SummarySection.Movement => "Движение",
            SummarySection.Oxygen => "Кислород",
            SummarySection.SocialStatus => "Социальный статус",
            SummarySection.Nozology => "Нозологии",
            SummarySection.Care => "Уход",
            _ => string.Empty
        };

        public static List<SummaryRow> Rows() => new()
        {
            // --- движение ---
            new() { Label = "Состояло", Section = SummarySection.Movement,
                    Adults = r => r.existed, Children = r => r.existedChildren },
            new() { Label = "Поступило", Section = SummarySection.Movement,
                    Adults = r => r.income, Children = r => r.incomeChildrens },
            new() { Label = "Выбыло", Section = SummarySection.Movement,
                    Adults = r => r.outcome, Children = r => r.outcomeChildrens },
            new() { Label = "Прикреплено в ОРИТ", Section = SummarySection.Movement,
                    Adults = r => r.attachedToORIT, Children = r => r.attachedToORITCildrens },
            new() { Label = "Переведено из отделения", Section = SummarySection.Movement,
                    Adults = r => r.movedOutDep, Children = r => r.movedOutDepChildrens },
            new() { Label = "Переведено в отделение", Section = SummarySection.Movement,
                    Adults = r => r.movedInDep, Children = r => r.movedInDepChildrens },
            new() { Label = "Умерло", Section = SummarySection.Movement,
                    Adults = r => r.died, Children = r => r.diedChildrens },
            new() { Label = "Состоит в отделении", Section = SummarySection.Movement, IsSubtotal = true,
                    Adults = r => r.present, Children = r => r.presentChildrens },

            // --- кислород ---
            new() { Label = "ИВЛ", Section = SummarySection.Oxygen,
                    Adults = r => r.oIVL, Children = r => r.oIVLChildrens },
            new() { Label = "НИВЛ", Section = SummarySection.Oxygen,
                    Adults = r => r.oNIVL, Children = r => r.oNIVLChildrens },
            new() { Label = "НИВЛ+ВПО", Section = SummarySection.Oxygen,
                    Adults = r => r.oNIVLVPO, Children = r => r.oNIVLVPOChildrens },
            new() { Label = "НИВЛ+КМ", Section = SummarySection.Oxygen,
                    Adults = r => r.oNIVLMask, Children = r => r.oNIVLMaskChildrens },
            new() { Label = "Лицевая маска", Section = SummarySection.Oxygen,
                    Adults = r => r.oMask, Children = r => r.oMaskChildren },
            new() { Label = string.Empty, Section = SummarySection.Oxygen, ConfigGroup = SummaryConfigGroup.Oxygen,
                    Adults = r => r.optionalOxygen, Children = r => r.optionalOxygenChildren },
            new() { Label = "Количество пациентов на кислороде", Section = SummarySection.Oxygen,
                    IsSubtotal = true, IsMerged = true, Merged = r => r.CountO2() },

            // --- социальный статус ---
            new() { Label = string.Empty, Section = SummarySection.SocialStatus, ConfigGroup = SummaryConfigGroup.Social,
                    Adults = r => r.optionalSocial, Children = r => r.optionalSocialChildren },
            new() { Label = "Беременные", Section = SummarySection.SocialStatus,
                    Adults = r => r.pregnant, Children = r => r.pregnantChildrens },
            new() { Label = "Пациенты из зон отдыха", Section = SummarySection.SocialStatus,
                    Adults = r => r.restZone, Children = r => r.restZoneChildrens },
            new() { Label = "Инокраевые", Section = SummarySection.SocialStatus,
                    Adults = r => r.outRegions, Children = r => r.outRegionsChildrens },
            new() { Label = "Иностранные граждане", Section = SummarySection.SocialStatus,
                    Adults = r => r.forein, Children = r => r.foreinChildrens },
            new() { Label = "Прибывшие из присоединённых областей (ЛНР, ДНР, ХО, ЗО)", Section = SummarySection.SocialStatus,
                    Adults = r => r.LNR_DNR, Children = r => r.LNR_DNRChildrens },
            new() { Label = "Прибывшие из иных территорий Украины", Section = SummarySection.SocialStatus,
                    Adults = r => r.otherUkrane, Children = r => r.otherUkraneChildren },
            new() { Label = "Всего пациентов, прибывших с территории Украины", Section = SummarySection.SocialStatus,
                    IsSubtotal = true, Adults = r => r.CountUcrane(), Children = r => r.CountUkraneChildren() },
            new() { Label = "Поступили переводом из другой МО", Section = SummarySection.SocialStatus,
                    Adults = r => r.incomeHospital, Children = r => r.incomeHospitalChildrens },
            new() { Label = "Переведены в другую МО", Section = SummarySection.SocialStatus,
                    Adults = r => r.outcomeHospital, Children = r => r.outcomeHospitalChildrens },

            // --- нозологии ---
            new() { Label = "U 07.1 Covid-19", Section = SummarySection.Nozology,
                    Adults = r => r.U071, Children = r => r.U071Childrens },
            new() { Label = "U 07.2 Covid-19", Section = SummarySection.Nozology,
                    Adults = r => r.U072, Children = r => r.U072Childrens },
            new() { Label = "ОРИ", Section = SummarySection.Nozology,
                    Adults = r => r.ORVI, Children = r => r.ORVIChildrens },
            new() { Label = "Пневмония", Section = SummarySection.Nozology,
                    Adults = r => r.pneumonia, Children = r => r.pneumoniaChildrens },
            new() { Label = "Грипп", Section = SummarySection.Nozology,
                    Adults = r => r.grippe, Children = r => r.grippeChildrens },
            new() { Label = "Корь", Section = SummarySection.Nozology,
                    Adults = r => r.measles, Children = r => r.measlesChildren },
            new() { Label = "ОКИ", Section = SummarySection.Nozology,
                    Adults = r => r.OKI, Children = r => r.OKIChildrens },
            new() { Label = "Менингит, энцефалит, миелит", Section = SummarySection.Nozology,
                    Adults = r => r.meningit, Children = r => r.meningitChildrens },
            new() { Label = "Сепсис", Section = SummarySection.Nozology,
                    Adults = r => r.sepsis, Children = r => r.sepsisChildren },
            new() { Label = "Гепатит", Section = SummarySection.Nozology,
                    Adults = r => r.hepatit, Children = r => r.hepatitChildrens },
            new() { Label = "В-20", Section = SummarySection.Nozology,
                    Adults = r => r.HIV, Children = r => r.HIVCildrens },
            new() { Label = string.Empty, Section = SummarySection.Nozology, ConfigGroup = SummaryConfigGroup.Nozology,
                    Adults = r => r.optionalNozology, Children = r => r.optionalNozologyChildren },
            new() { Label = "Прочие", Section = SummarySection.Nozology,
                    Adults = r => r.other, Children = r => r.otherChildrens },

            // --- уход ---
            new() { Label = "Лица по уходу", Section = SummarySection.Care, IsMerged = true,
                    Merged = r => r.care },
            new() { Label = "Лица по уходу до 4х лет или с детьми инвалидами", Section = SummarySection.Care, IsMerged = true,
                    Merged = r => r.careDisodered },
            new() { Label = string.Empty, Section = SummarySection.Care, IsMerged = true, ConfigGroup = SummaryConfigGroup.Care,
                    Merged = r => r.optionalCare },
            new() { Label = "Количество человек в круглосуточном стационаре", Section = SummarySection.Care, IsSubtotal = true,
                    Adults = r => r.CountPresentCare(), Children = r => r.presentChildrens }
        };
    }
}
