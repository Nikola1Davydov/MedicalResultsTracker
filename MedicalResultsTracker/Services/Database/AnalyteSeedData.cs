using MedicalResultsTracker.Model;

namespace MedicalResultsTracker.Services.Database
{
    /// <summary>
    /// Встроенный справочник частых показателей.
    /// ВАЖНО: диапазоны здесь — ориентировочные, для взрослых, чтобы не вбивать их руками каждый раз.
    /// У каждой лаборатории нормы свои, поэтому при вводе анализа приоритет всегда у значений с бланка,
    /// а любую строку каталога можно отредактировать в настройках.
    /// </summary>
    internal static class AnalyteSeedData
    {
        private const string Cbc = "Общий анализ крови";
        private const string Chem = "Биохимия";
        private const string Lipids = "Липиды";
        private const string Iron = "Обмен железа";
        private const string Vitamins = "Витамины";
        private const string Hormones = "Гормоны";
        private const string Inflammation = "Воспаление";

        internal static IReadOnlyList<Analyte> BuiltIn { get; } = new List<Analyte>
        {
            New("WBC", "Лейкоциты (WBC)", "10⁹/л", Cbc, 4.0, 9.0, order: 10),
            New("RBC", "Эритроциты (RBC)", "10¹²/л", Cbc, 3.9, 5.6, order: 20, notes: "У женщин нижняя граница обычно ниже"),
            New("HGB", "Гемоглобин (HGB)", "г/л", Cbc, 120, 170, order: 30, notes: "Ж: 120–150, М: 130–170"),
            New("HCT", "Гематокрит (HCT)", "%", Cbc, 36, 50, order: 40),
            New("PLT", "Тромбоциты (PLT)", "10⁹/л", Cbc, 150, 400, order: 50),
            New("MCV", "Средний объём эритроцита (MCV)", "фл", Cbc, 80, 100, order: 60),
            New("MCH", "Среднее содержание Hb (MCH)", "пг", Cbc, 27, 34, order: 70),
            New("ESR", "СОЭ", "мм/ч", Cbc, null, 20, order: 80),

            New("GLU", "Глюкоза", "ммоль/л", Chem, 3.9, 5.9, order: 10, notes: "Натощак"),
            New("HBA1C", "Гликированный гемоглобин (HbA1c)", "%", Chem, null, 5.7, order: 20),
            New("ALT", "АЛТ", "Ед/л", Chem, null, 41, order: 30),
            New("AST", "АСТ", "Ед/л", Chem, null, 40, order: 40),
            New("GGT", "ГГТ", "Ед/л", Chem, null, 60, order: 50),
            New("BILT", "Билирубин общий", "мкмоль/л", Chem, 3.4, 20.5, order: 60),
            New("CREA", "Креатинин", "мкмоль/л", Chem, 62, 106, order: 70),
            New("UREA", "Мочевина", "ммоль/л", Chem, 2.8, 7.2, order: 80),
            New("UA", "Мочевая кислота", "мкмоль/л", Chem, 200, 420, order: 90),
            New("TP", "Общий белок", "г/л", Chem, 64, 83, order: 100),

            New("CHOL", "Холестерин общий", "ммоль/л", Lipids, null, 5.2, order: 10),
            New("LDL", "ЛПНП (LDL)", "ммоль/л", Lipids, null, 3.0, order: 20, notes: "Цель зависит от сердечно-сосудистого риска"),
            New("HDL", "ЛПВП (HDL)", "ммоль/л", Lipids, 1.0, null, order: 30),
            New("TG", "Триглицериды", "ммоль/л", Lipids, null, 1.7, order: 40),

            New("FERR", "Ферритин", "мкг/л", Iron, 30, 300, order: 10),
            New("FE", "Железо сывороточное", "мкмоль/л", Iron, 10.7, 32.2, order: 20),
            New("TSAT", "Насыщение трансферрина", "%", Iron, 20, 50, order: 30),
            New("TRF", "Трансферрин", "г/л", Iron, 2.0, 3.6, order: 40),

            New("VITD", "Витамин D (25-OH)", "нг/мл", Vitamins, 30, 100, order: 10),
            New("B12", "Витамин B12", "пг/мл", Vitamins, 200, 900, order: 20),
            New("FOL", "Фолиевая кислота", "нг/мл", Vitamins, 3.0, 17.0, order: 30),
            New("MG", "Магний", "ммоль/л", Vitamins, 0.66, 1.07, order: 40),

            New("TSH", "ТТГ", "мЕд/л", Hormones, 0.4, 4.0, order: 10),
            New("FT4", "Т4 свободный", "пмоль/л", Hormones, 12, 22, order: 20),
            New("FT3", "Т3 свободный", "пмоль/л", Hormones, 3.1, 6.8, order: 30),
            New("TSTO", "Тестостерон общий", "нмоль/л", Hormones, 8.6, 29.0, order: 40, notes: "Диапазон для мужчин"),
            New("CORT", "Кортизол", "нмоль/л", Hormones, 138, 635, order: 50, notes: "Утренний забор"),

            New("CRP", "С-реактивный белок (СРБ)", "мг/л", Inflammation, null, 5.0, order: 10),
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
