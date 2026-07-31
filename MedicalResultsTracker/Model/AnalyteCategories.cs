namespace MedicalResultsTracker.Model
{
    /// <summary>
    /// Группы встроенного справочника.
    ///
    /// В базе лежит именно эта строка, одна и та же при любом языке интерфейса: группа —
    /// ключ группировки, и локализованное значение развалило бы её на «Липиды» и «Blutfette»
    /// у одного и того же пользователя после смены языка. Перевод происходит только
    /// при выводе на экран, см. <c>AnalyteDisplay</c>.
    /// </summary>
    public static class AnalyteCategories
    {
        public const string Cbc = "Blutbild";
        public const string Liver = "Leberwerte";
        public const string Kidney = "Nierenwerte";
        public const string Lipids = "Blutfette";
        public const string Metabolism = "Stoffwechsel";
        public const string Iron = "Eisenstoffwechsel";
        public const string Vitamins = "Vitamine";
        public const string Electrolytes = "Elektrolyte";
        public const string Thyroid = "Schilddrüse";
        public const string Hormones = "Hormone";
        public const string Inflammation = "Entzündung";

        /// <summary>Куда попадают показатели, добавленные пользователем.</summary>
        public const string Own = "Meine Werte";

        /// <summary>Ключ ресурса для перевода названия группы; null — группа придумана пользователем.</summary>
        public static string? ResourceKey(string? category) => category?.Trim() switch
        {
            Cbc => "Cat_Group_Cbc",
            Liver => "Cat_Group_Liver",
            Kidney => "Cat_Group_Kidney",
            Lipids => "Cat_Group_Lipids",
            Metabolism => "Cat_Group_Metabolism",
            Iron => "Cat_Group_Iron",
            Vitamins => "Cat_Group_Vitamins",
            Electrolytes => "Cat_Group_Electrolytes",
            Thyroid => "Cat_Group_Thyroid",
            Hormones => "Cat_Group_Hormones",
            Inflammation => "Cat_Group_Inflammation",
            Own => "Cat_Group_Own",
            _ => null
        };
    }
}
