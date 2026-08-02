using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.Services.Database
{
    /// <summary>
    /// Встроенный справочник частых показателей — в том виде, в каком они печатаются
    /// в немецких лабораторных бланках: немецкие названия, немецкие единицы
    /// (Hämoglobin в g/dl, Glukose и Cholesterin в mg/dl), немецкие группы.
    ///
    /// ВАЖНО: диапазоны ориентировочные, для взрослых, и нужны только чтобы не вбивать их
    /// руками каждый раз. У каждой лаборатории границы свои, поэтому подставленные значения
    /// правятся прямо в строке анализа, и в базу уходит то, что напечатано в вашем бланке.
    ///
    /// Названия групп намеренно не переводятся: они хранятся в базе и должны совпадать
    /// у всех языков интерфейса, иначе группировка развалится при смене локали.
    /// На экране их переводит <see cref="Resources.Strings.AnalyteDisplay"/> — так же,
    /// как примечания: в <c>notes</c> здесь стоит ключ ресурса, а не готовый текст.
    /// </summary>
    internal static class AnalyteSeedData
    {
        /// <summary>
        /// Версия набора. Увеличивается, когда встроенные записи меняются, — тогда
        /// каталог обновляется у тех, кто уже пользуется приложением.
        /// </summary>
        internal const int Version = 2;

        private const string Cbc = AnalyteCategories.Cbc;
        private const string Liver = AnalyteCategories.Liver;
        private const string Kidney = AnalyteCategories.Kidney;
        private const string Lipids = AnalyteCategories.Lipids;
        private const string Metabolism = AnalyteCategories.Metabolism;
        private const string Iron = AnalyteCategories.Iron;
        private const string Vitamins = AnalyteCategories.Vitamins;
        private const string Electrolytes = AnalyteCategories.Electrolytes;
        private const string Thyroid = AnalyteCategories.Thyroid;
        private const string Hormones = AnalyteCategories.Hormones;
        private const string Inflammation = AnalyteCategories.Inflammation;

        internal static IReadOnlyList<Analyte> BuiltIn { get; } = new List<Analyte>
        {
            New("WBC", "Leukozyten", "/nl", Cbc, 4.0, 10.0, order: 10),
            New("RBC", "Erythrozyten", "/pl", Cbc, 4.3, 5.8, order: 20, notes: "Seed_Note_Rbc"),
            New("HGB", "Hämoglobin", "g/dl", Cbc, 12.0, 17.5, order: 30, notes: "Seed_Note_Hgb"),
            New("HCT", "Hämatokrit", "%", Cbc, 37, 50, order: 40),
            New("PLT", "Thrombozyten", "/nl", Cbc, 150, 400, order: 50),
            New("MCV", "MCV", "fl", Cbc, 80, 96, order: 60),
            New("MCH", "MCH", "pg", Cbc, 28, 33, order: 70),
            New("ESR", "BSG (Blutsenkung)", "mm/h", Cbc, null, 20, order: 80),

            New("ALT", "GPT (ALT)", "U/l", Liver, null, 50, order: 10),
            New("AST", "GOT (AST)", "U/l", Liver, null, 50, order: 20),
            New("GGT", "Gamma-GT", "U/l", Liver, null, 60, order: 30),
            New("BILT", "Bilirubin gesamt", "mg/dl", Liver, 0.1, 1.2, order: 40),

            New("CREA", "Kreatinin", "mg/dl", Kidney, 0.7, 1.2, order: 10),
            New("EGFR", "eGFR", "ml/min", Kidney, 90, null, order: 20),
            New("UREA", "Harnstoff", "mg/dl", Kidney, 17, 43, order: 30),
            New("UA", "Harnsäure", "mg/dl", Kidney, 3.4, 7.0, order: 40),

            New("CHOL", "Cholesterin gesamt", "mg/dl", Lipids, null, 200, order: 10),
            New("LDL", "LDL-Cholesterin", "mg/dl", Lipids, null, 116, order: 20, notes: "Seed_Note_Ldl"),
            New("HDL", "HDL-Cholesterin", "mg/dl", Lipids, 40, null, order: 30, notes: "Seed_Note_Hdl"),
            New("TG", "Triglyzeride", "mg/dl", Lipids, null, 150, order: 40),

            New("GLU", "Glukose", "mg/dl", Metabolism, 70, 100, order: 10, notes: "Seed_Note_Glu"),
            New("HBA1C", "HbA1c", "%", Metabolism, null, 5.7, order: 20),
            New("TP", "Gesamteiweiß", "g/l", Metabolism, 66, 83, order: 30),

            New("FERR", "Ferritin", "ng/ml", Iron, 30, 300, order: 10),
            New("FE", "Eisen", "µg/dl", Iron, 60, 180, order: 20),
            New("TRF", "Transferrin", "mg/dl", Iron, 200, 360, order: 30),
            New("TSAT", "Transferrinsättigung", "%", Iron, 16, 45, order: 40),

            New("VITD", "Vitamin D (25-OH)", "ng/ml", Vitamins, 30, 100, order: 10),
            New("B12", "Vitamin B12", "pg/ml", Vitamins, 200, 900, order: 20),
            New("FOL", "Folsäure", "ng/ml", Vitamins, 3.0, 17.0, order: 30),

            New("NA", "Natrium", "mmol/l", Electrolytes, 135, 145, order: 10),
            New("K", "Kalium", "mmol/l", Electrolytes, 3.5, 5.1, order: 20),
            New("CA", "Calcium", "mmol/l", Electrolytes, 2.2, 2.6, order: 30),
            New("MG", "Magnesium", "mmol/l", Electrolytes, 0.7, 1.1, order: 40),

            New("TSH", "TSH", "mU/l", Thyroid, 0.4, 4.0, order: 10),
            New("FT4", "fT4", "ng/dl", Thyroid, 0.9, 1.7, order: 20),
            New("FT3", "fT3", "pg/ml", Thyroid, 2.0, 4.4, order: 30),

            New("TSTO", "Testosteron gesamt", "ng/ml", Hormones, 2.8, 8.0, order: 10, notes: "Seed_Note_Tsto"),
            New("CORT", "Cortisol", "µg/dl", Hormones, 5, 25, order: 20, notes: "Seed_Note_Cort"),

            New("CRP", "CRP", "mg/l", Inflammation, null, 5.0, order: 10),
        };

        private static Analyte New(
            string code,
            string name,
            string unit,
            string category,
            double? min,
            double? max,
            int order,
            string? notes = null) => new()
            {
                Code = code,
                Name = name,
                Unit = unit,
                Category = category,
                RefMin = min,
                RefMax = max,
                Notes = notes,
                IsBuiltIn = true,
                SortOrder = order,
            };
    }
}
