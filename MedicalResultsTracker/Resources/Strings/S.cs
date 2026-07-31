using System.Globalization;
using System.Resources;

namespace MedicalResultsTracker.Resources.Strings
{
    /// <summary>
    /// Тексты интерфейса. Немецкий — язык по умолчанию, русский подхватывается,
    /// если он выбран в настройках или стоит в системе.
    /// Файл сгенерирован из AppResources.resx: править нужно resx, а не его.
    /// </summary>
    public static class S
    {
        private static readonly ResourceManager Manager =
            new("MedicalResultsTracker.Resources.Strings.AppResources", typeof(S).Assembly);

        /// <summary>Строка по ключу. Отсутствующий ключ виден сразу: возвращается сам ключ.</summary>
        public static string Get(string key) =>
            Manager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

        /// <summary>Строка по ключу или null, если ключа нет. Для необязательных переводов.</summary>
        public static string? Find(string? key) =>
            string.IsNullOrEmpty(key) ? null : Manager.GetString(key, CultureInfo.CurrentUICulture);

        /// <summary>Laborwerte</summary>
        public static string App_Title => Get("App_Title");

        /// <summary>Übersicht</summary>
        public static string Tab_Dashboard => Get("Tab_Dashboard");

        /// <summary>Befunde</summary>
        public static string Tab_History => Get("Tab_History");

        /// <summary>Verlauf</summary>
        public static string Tab_Trends => Get("Tab_Trends");

        /// <summary>Einstellungen</summary>
        public static string Tab_Settings => Get("Tab_Settings");

        /// <summary>OK</summary>
        public static string Common_Ok => Get("Common_Ok");

        /// <summary>Abbrechen</summary>
        public static string Common_Cancel => Get("Common_Cancel");

        /// <summary>Löschen</summary>
        public static string Common_Delete => Get("Common_Delete");

        /// <summary>Speichern</summary>
        public static string Common_Save => Get("Common_Save");

        /// <summary>Fehler</summary>
        public static string Common_Error => Get("Common_Error");

        /// <summary>—</summary>
        public static string Common_None => Get("Common_None");

        /// <summary>Meine Laborwerte</summary>
        public static string Dash_Title => Get("Dash_Title");

        /// <summary>Noch kein Befund erfasst</summary>
        public static string Dash_NoTests => Get("Dash_NoTests");

        /// <summary>Letzter Befund: {0}</summary>
        public static string Dash_LastTest => Get("Dash_LastTest");

        /// <summary>Erfassen Sie den ersten Befund – den Verlauf zeigt die App dann von se</summary>
        public static string Dash_EmptyHint => Get("Dash_EmptyHint");

        /// <summary>➕ Befund erfassen</summary>
        public static string Dash_AddTest => Get("Dash_AddTest");

        /// <summary>⬇ Exportieren</summary>
        public static string Dash_Export => Get("Dash_Export");

        /// <summary>So fangen Sie an</summary>
        public static string Dash_StartTitle => Get("Dash_StartTitle");

        /// <summary>Nehmen Sie Ihren Laborbefund und tragen Sie die Werte ein: Bezeichnung</summary>
        public static string Dash_StartBody => Get("Dash_StartBody");

        /// <summary>Alles wird lokal auf diesem Gerät gespeichert. Es werden keine Daten ü</summary>
        public static string Dash_StartNote => Get("Dash_StartNote");

        /// <summary>★ Favoriten</summary>
        public static string Dash_Favorites => Get("Dash_Favorites");

        /// <summary>Werte im Blick behalten</summary>
        public static string Dash_FavHintTitle => Get("Dash_FavHintTitle");

        /// <summary>Markieren Sie im Werteverzeichnis mit dem Stern, was Sie dauerhaft ver</summary>
        public static string Dash_FavHintBody => Get("Dash_FavHintBody");

        /// <summary>Werteverzeichnis öffnen</summary>
        public static string Dash_OpenCatalog => Get("Dash_OpenCatalog");

        /// <summary>Außerhalb des Referenzbereichs</summary>
        public static string Dash_OutOfRange => Get("Dash_OutOfRange");

        /// <summary>Deutliche Veränderungen</summary>
        public static string Dash_Changes => Get("Dash_Changes");

        /// <summary>KI fragen</summary>
        public static string Dash_AiTitle => Get("Dash_AiTitle");

        /// <summary>Die App stellt die Tabelle als Text zusammen und öffnet die Teilen-Aus</summary>
        public static string Dash_AiBody => Get("Dash_AiBody");

        /// <summary>🤖 Tabelle an KI-Chat senden</summary>
        public static string Dash_AiButton => Get("Dash_AiButton");

        /// <summary>KI-Assistent ist aus. Die Daten verlassen das Gerät nicht.</summary>
        public static string Dash_AiOff => Get("Dash_AiOff");

        /// <summary>KI-Assistent: {0}. Die Berechtigung lässt sich in den Einstellungen wi</summary>
        public static string Dash_AiOn => Get("Dash_AiOn");

        /// <summary>{0} Werte · {1} · {2}</summary>
        public static string Dash_Summary => Get("Dash_Summary");

        /// <summary>alle Werte im Referenzbereich</summary>
        public static string Dash_AllInRange => Get("Dash_AllInRange");

        /// <summary>1 Wert außerhalb</summary>
        public static string Dash_OneOut => Get("Dash_OneOut");

        /// <summary>{0} Werte außerhalb</summary>
        public static string Dash_ManyOut => Get("Dash_ManyOut");

        /// <summary>1 Befund im Verlauf</summary>
        public static string Dash_OneTest => Get("Dash_OneTest");

        /// <summary>{0} Befunde im Verlauf</summary>
        public static string Dash_ManyTests => Get("Dash_ManyTests");

        /// <summary>Hier erscheinen alle gespeicherten Befunde – einer pro Blutabnahme.</summary>
        public static string Hist_Empty => Get("Hist_Empty");

        /// <summary>➕ Befund erfassen</summary>
        public static string Hist_Add => Get("Hist_Add");

        /// <summary>⬇ Tabelle exportieren</summary>
        public static string Hist_Export => Get("Hist_Export");

        /// <summary>{0} Werte · alle im Referenzbereich</summary>
        public static string Hist_SubtitleOk => Get("Hist_SubtitleOk");

        /// <summary>{0} Werte · {1} außerhalb</summary>
        public static string Hist_SubtitleOut => Get("Hist_SubtitleOut");

        /// <summary>Neuer Befund</summary>
        public static string Edit_TitleNew => Get("Edit_TitleNew");

        /// <summary>Befund</summary>
        public static string Edit_TitleExisting => Get("Edit_TitleExisting");

        /// <summary>Datum der Abnahme</summary>
        public static string Edit_Date => Get("Edit_Date");

        /// <summary>Labor (optional)</summary>
        public static string Edit_Lab => Get("Edit_Lab");

        /// <summary>z. B. Synlab</summary>
        public static string Edit_LabPlaceholder => Get("Edit_LabPlaceholder");

        /// <summary>Notiz (optional)</summary>
        public static string Edit_Note => Get("Edit_Note");

        /// <summary>nüchtern, nach Vitaminkur …</summary>
        public static string Edit_NotePlaceholder => Get("Edit_NotePlaceholder");

        /// <summary>Wert hinzufügen</summary>
        public static string Edit_AddParam => Get("Edit_AddParam");

        /// <summary>Bezeichnung eingeben</summary>
        public static string Edit_SearchPlaceholder => Get("Edit_SearchPlaceholder");

        /// <summary>So übernehmen</summary>
        public static string Edit_AddTyped => Get("Edit_AddTyped");

        /// <summary>Leere Zeile</summary>
        public static string Edit_AddEmpty => Get("Edit_AddEmpty");

        /// <summary>Wie beim letzten Mal</summary>
        public static string Edit_CopyPrevious => Get("Edit_CopyPrevious");

        /// <summary>Aus einem Foto ausfüllen</summary>
        public static string Edit_PhotoTitle => Get("Edit_PhotoTitle");

        /// <summary>Den Befund liest Ihr KI-Chat, nicht die App. Anfrage kopieren, zusamme</summary>
        public static string Edit_PhotoBody => Get("Edit_PhotoBody");

        /// <summary>1. Anfrage kopieren</summary>
        public static string Edit_CopyPrompt => Get("Edit_CopyPrompt");

        /// <summary>2. Aus Zwischenablage</summary>
        public static string Edit_Paste => Get("Edit_Paste");

        /// <summary>Die App sendet dabei nichts: Der Austausch mit dem Chat läuft von Hand</summary>
        public static string Edit_PhotoNote => Get("Edit_PhotoNote");

        /// <summary>Bezeichnung</summary>
        public static string Edit_RowName => Get("Edit_RowName");

        /// <summary>Wert</summary>
        public static string Edit_RowValue => Get("Edit_RowValue");

        /// <summary>Einheit</summary>
        public static string Edit_RowUnit => Get("Edit_RowUnit");

        /// <summary>Referenz von</summary>
        public static string Edit_RowRefMin => Get("Edit_RowRefMin");

        /// <summary>Referenz bis</summary>
        public static string Edit_RowRefMax => Get("Edit_RowRefMax");

        /// <summary>Zahlen dürfen mit Komma oder Punkt geschrieben werden. Den Referenzber</summary>
        public static string Edit_Hint => Get("Edit_Hint");

        /// <summary>Keine Daten</summary>
        public static string Edit_NoPrevTitle => Get("Edit_NoPrevTitle");

        /// <summary>Es gibt keinen früheren Befund.</summary>
        public static string Edit_NoPrevBody => Get("Edit_NoPrevBody");

        /// <summary>Leer</summary>
        public static string Edit_EmptyTitle => Get("Edit_EmptyTitle");

        /// <summary>Fügen Sie mindestens einen Wert hinzu.</summary>
        public static string Edit_EmptyBody => Get("Edit_EmptyBody");

        /// <summary>Bezeichnung fehlt</summary>
        public static string Edit_NoNameTitle => Get("Edit_NoNameTitle");

        /// <summary>Jede ausgefüllte Zeile braucht eine Bezeichnung.</summary>
        public static string Edit_NoNameBody => Get("Edit_NoNameBody");

        /// <summary>Befund löschen?</summary>
        public static string Edit_DeleteTitle => Get("Edit_DeleteTitle");

        /// <summary>Der Eintrag wird unwiderruflich vom Gerät gelöscht.</summary>
        public static string Edit_DeleteBody => Get("Edit_DeleteBody");

        /// <summary>Anfrage kopiert</summary>
        public static string Edit_PromptCopiedTitle => Get("Edit_PromptCopiedTitle");

        /// <summary>Fügen Sie sie zusammen mit dem Foto des Befunds in einen beliebigen KI</summary>
        public static string Edit_PromptCopiedBody => Get("Edit_PromptCopiedBody");

        /// <summary>Zwischenablage ist leer</summary>
        public static string Edit_ClipEmptyTitle => Get("Edit_ClipEmptyTitle");

        /// <summary>Kopieren Sie zuerst die Tabelle mit den Werten.</summary>
        public static string Edit_ClipEmptyBody => Get("Edit_ClipEmptyBody");

        /// <summary>Nichts erkannt</summary>
        public static string Edit_NoRowsTitle => Get("Edit_NoRowsTitle");

        /// <summary>Im Text wurden keine Wertezeilen gefunden. Eine Zeile sollte so ausseh</summary>
        public static string Edit_NoRowsBody => Get("Edit_NoRowsBody");

        /// <summary>{0} Zeilen übernommen. Bitte vor dem Speichern prüfen.</summary>
        public static string Edit_Added => Get("Edit_Added");

        /// <summary>{0} Nicht erkannt: {1}</summary>
        public static string Edit_AddedWarn => Get("Edit_AddedWarn");

        /// <summary>Meine Werte</summary>
        public static string Edit_MyValues => Get("Edit_MyValues");

        /// <summary>Diagramme erscheinen, sobald zwei Befunde mit denselben Werten vorlieg</summary>
        public static string Trend_Empty => Get("Trend_Empty");

        /// <summary>★ Nur Favoriten</summary>
        public static string Trend_OnlyFavorites => Get("Trend_OnlyFavorites");

        /// <summary>Alle / mit Verlauf</summary>
        public static string Trend_AllOrHistory => Get("Trend_AllOrHistory");

        /// <summary>★ Favoriten</summary>
        public static string Trend_FavoritesGroup => Get("Trend_FavoritesGroup");

        /// <summary>Ohne Gruppe</summary>
        public static string Trend_NoGroup => Get("Trend_NoGroup");

        /// <summary>eine Messung</summary>
        public static string Trend_OneMeasurement => Get("Trend_OneMeasurement");

        /// <summary>→ unverändert</summary>
        public static string Trend_NoChange => Get("Trend_NoChange");

        /// <summary>Werte</summary>
        public static string Trend_Values => Get("Trend_Values");

        /// <summary>Referenz: {0} {1}</summary>
        public static string Trend_RefKnown => Get("Trend_RefKnown");

        /// <summary>Kein Referenzbereich angegeben</summary>
        public static string Trend_RefUnknown => Get("Trend_RefUnknown");

        /// <summary>Eine Messung – der Verlauf entsteht ab dem nächsten Befund.</summary>
        public static string Trend_SinglePoint => Get("Trend_SinglePoint");

        /// <summary>{0} Messungen seit {1}</summary>
        public static string Trend_Since => Get("Trend_Since");

        /// <summary>Zu diesem Wert liegen keine Daten vor.</summary>
        public static string Trend_NoData => Get("Trend_NoData");

        /// <summary>Keine Daten</summary>
        public static string Trend_ChartNoData => Get("Trend_ChartNoData");

        /// <summary>Wert</summary>
        public static string Trend_Parameter => Get("Trend_Parameter");

        /// <summary>unter dem Referenzbereich</summary>
        public static string Status_Low => Get("Status_Low");

        /// <summary>über dem Referenzbereich</summary>
        public static string Status_High => Get("Status_High");

        /// <summary>im Referenzbereich</summary>
        public static string Status_Normal => Get("Status_Normal");

        /// <summary>kein Referenzbereich</summary>
        public static string Status_Unknown => Get("Status_Unknown");

        /// <summary>besser als beim letzten Mal</summary>
        public static string Assess_Improved => Get("Assess_Improved");

        /// <summary>schlechter als beim letzten Mal</summary>
        public static string Assess_Worsened => Get("Assess_Worsened");

        /// <summary>unverändert</summary>
        public static string Assess_Stable => Get("Assess_Stable");

        /// <summary>kein Vergleich möglich</summary>
        public static string Assess_Unknown => Get("Assess_Unknown");

        /// <summary>Referenz {0}</summary>
        public static string Item_RefKnown => Get("Item_RefKnown");

        /// <summary>kein Referenzbereich</summary>
        public static string Item_RefUnknown => Get("Item_RefUnknown");

        /// <summary>zuvor {0} · {1}</summary>
        public static string Item_Previous => Get("Item_Previous");

        /// <summary>erste Messung</summary>
        public static string Item_First => Get("Item_First");

        /// <summary>Werteverzeichnis</summary>
        public static string Cat_Title => Get("Cat_Title");

        /// <summary>Suche nach Bezeichnung</summary>
        public static string Cat_Search => Get("Cat_Search");

        /// <summary>Nur Favoriten</summary>
        public static string Cat_OnlyFavorites => Get("Cat_OnlyFavorites");

        /// <summary>Ausgeblendete zeigen</summary>
        public static string Cat_ShowHidden => Get("Cat_ShowHidden");

        /// <summary>➕ Eigener Wert</summary>
        public static string Cat_AddOwn => Get("Cat_AddOwn");

        /// <summary>aus den Vorschlägen ausgeblendet</summary>
        public static string Cat_Hidden => Get("Cat_Hidden");

        /// <summary>{0} Werte · {1} als Favorit</summary>
        public static string Cat_Summary => Get("Cat_Summary");

        /// <summary>{0} Werte · {1} als Favorit · {2} ausgeblendet</summary>
        public static string Cat_SummaryHidden => Get("Cat_SummaryHidden");

        /// <summary>nicht verwendet</summary>
        public static string Cat_Unused => Get("Cat_Unused");

        /// <summary>1 Messung</summary>
        public static string Cat_OneMeasurement => Get("Cat_OneMeasurement");

        /// <summary>{0} Messungen</summary>
        public static string Cat_ManyMeasurements => Get("Cat_ManyMeasurements");

        /// <summary>vorgegeben</summary>
        public static string Cat_BuiltIn => Get("Cat_BuiltIn");

        /// <summary>eigener</summary>
        public static string Cat_Own => Get("Cat_Own");

        /// <summary>kein Referenzbereich</summary>
        public static string Cat_NoRef => Get("Cat_NoRef");

        /// <summary>1 Wert</summary>
        public static string Cat_OneParam => Get("Cat_OneParam");

        /// <summary>{0} Werte</summary>
        public static string Cat_ManyParams => Get("Cat_ManyParams");

        /// <summary>Wert</summary>
        public static string CatEdit_Title => Get("CatEdit_Title");

        /// <summary>Neuer Wert</summary>
        public static string CatEdit_TitleNew => Get("CatEdit_TitleNew");

        /// <summary>Bezeichnung</summary>
        public static string CatEdit_Name => Get("CatEdit_Name");

        /// <summary>z. B. Homocystein</summary>
        public static string CatEdit_NamePlaceholder => Get("CatEdit_NamePlaceholder");

        /// <summary>Einheit</summary>
        public static string CatEdit_Unit => Get("CatEdit_Unit");

        /// <summary>µmol/l</summary>
        public static string CatEdit_UnitPlaceholder => Get("CatEdit_UnitPlaceholder");

        /// <summary>Gruppe</summary>
        public static string CatEdit_Group => Get("CatEdit_Group");

        /// <summary>Vorhandene wählen</summary>
        public static string CatEdit_GroupPick => Get("CatEdit_GroupPick");

        /// <summary>… oder neue eingeben</summary>
        public static string CatEdit_GroupNew => Get("CatEdit_GroupNew");

        /// <summary>Hinweis zum Referenzbereich</summary>
        public static string CatEdit_Notes => Get("CatEdit_Notes");

        /// <summary>z. B. Bereich für Männer</summary>
        public static string CatEdit_NotesPlaceholder => Get("CatEdit_NotesPlaceholder");

        /// <summary>Diese Angaben werden beim Erfassen nur vorgeschlagen. In gespeicherten</summary>
        public static string CatEdit_Note => Get("CatEdit_Note");

        /// <summary>Code: {0}</summary>
        public static string CatEdit_Code => Get("CatEdit_Code");

        /// <summary>Der Code entsteht aus der Bezeichnung</summary>
        public static string CatEdit_CodeNew => Get("CatEdit_CodeNew");

        /// <summary>★ Als Favorit</summary>
        public static string CatEdit_Favorite => Get("CatEdit_Favorite");

        /// <summary>Der Wert erscheint oben auf der Startseite und in einer eigenen Gruppe</summary>
        public static string CatEdit_FavoriteNote => Get("CatEdit_FavoriteNote");

        /// <summary>Aus Vorschlägen ausblenden</summary>
        public static string CatEdit_Hide => Get("CatEdit_Hide");

        /// <summary>Der Eintrag bleibt erhalten, wird beim Erfassen aber nicht mehr vorges</summary>
        public static string CatEdit_HideNote => Get("CatEdit_HideNote");

        /// <summary>Mit anderem Wert zusammenführen</summary>
        public static string CatEdit_MergeTitle => Get("CatEdit_MergeTitle");

        /// <summary>Steht derselbe Wert doppelt im Verzeichnis, verschieben Sie seine Mess</summary>
        public static string CatEdit_MergeBody => Get("CatEdit_MergeBody");

        /// <summary>Wohin verschieben</summary>
        public static string CatEdit_MergeTarget => Get("CatEdit_MergeTarget");

        /// <summary>Zusammenführen</summary>
        public static string CatEdit_Merge => Get("CatEdit_Merge");

        /// <summary>Zu diesem Wert gibt es noch keine Messungen.</summary>
        public static string CatEdit_NoUsage => Get("CatEdit_NoUsage");

        /// <summary>1 gespeicherte Messung.</summary>
        public static string CatEdit_OneUsage => Get("CatEdit_OneUsage");

        /// <summary>{0} gespeicherte Messungen.</summary>
        public static string CatEdit_ManyUsage => Get("CatEdit_ManyUsage");

        /// <summary>Bezeichnung fehlt</summary>
        public static string CatEdit_NoNameTitle => Get("CatEdit_NoNameTitle");

        /// <summary>Geben Sie eine Bezeichnung an.</summary>
        public static string CatEdit_NoNameBody => Get("CatEdit_NoNameBody");

        /// <summary>Gibt es bereits</summary>
        public static string CatEdit_ExistsTitle => Get("CatEdit_ExistsTitle");

        /// <summary>Ein Wert mit dieser Bezeichnung steht schon im Verzeichnis. Öffnen Sie</summary>
        public static string CatEdit_ExistsBody => Get("CatEdit_ExistsBody");

        /// <summary>Vorgegebener Wert</summary>
        public static string CatEdit_BuiltInTitle => Get("CatEdit_BuiltInTitle");

        /// <summary>Vorgegebene Werte lassen sich nicht löschen – sie kehren beim nächsten</summary>
        public static string CatEdit_BuiltInBody => Get("CatEdit_BuiltInBody");

        /// <summary>Aus dem Verzeichnis löschen?</summary>
        public static string CatEdit_DeleteTitle => Get("CatEdit_DeleteTitle");

        /// <summary>Der Verzeichniseintrag wird gelöscht.</summary>
        public static string CatEdit_DeleteBody => Get("CatEdit_DeleteBody");

        /// <summary>Der Verzeichniseintrag wird gelöscht. Die gespeicherten Messungen ({0}</summary>
        public static string CatEdit_DeleteBodyUsed => Get("CatEdit_DeleteBodyUsed");

        /// <summary>Wert wählen</summary>
        public static string CatEdit_MergePickTitle => Get("CatEdit_MergePickTitle");

        /// <summary>Geben Sie an, mit welchem Wert zusammengeführt werden soll.</summary>
        public static string CatEdit_MergePickBody => Get("CatEdit_MergePickBody");

        /// <summary>Werte zusammenführen?</summary>
        public static string CatEdit_MergeConfirmTitle => Get("CatEdit_MergeConfirmTitle");

        /// <summary>Die Messungen von «{0}» ({1}) gehen an «{2}» über und bilden künftig e</summary>
        public static string CatEdit_MergeConfirmBody => Get("CatEdit_MergeConfirmBody");

        /// <summary>Fertig</summary>
        public static string CatEdit_MergeDoneTitle => Get("CatEdit_MergeDoneTitle");

        /// <summary>Verschobene Messungen: {0}.</summary>
        public static string CatEdit_MergeDoneBody => Get("CatEdit_MergeDoneBody");

        /// <summary>Speicherung</summary>
        public static string Set_StorageTitle => Get("Set_StorageTitle");

        /// <summary>Noch keine Befunde. Die Daten liegen ausschließlich auf diesem Gerät.</summary>
        public static string Set_StorageEmpty => Get("Set_StorageEmpty");

        /// <summary>1 Befund. Die Daten liegen ausschließlich auf diesem Gerät.</summary>
        public static string Set_StorageOne => Get("Set_StorageOne");

        /// <summary>{0} Befunde. Die Daten liegen ausschließlich auf diesem Gerät.</summary>
        public static string Set_StorageMany => Get("Set_StorageMany");

        /// <summary>Datenbankdatei:</summary>
        public static string Set_DbFile => Get("Set_DbFile");

        /// <summary>Werteverzeichnis</summary>
        public static string Set_CatalogTitle => Get("Set_CatalogTitle");

        /// <summary>Was beim Erfassen vorgeschlagen wird: Bezeichnungen, Einheiten und übl</summary>
        public static string Set_CatalogBody => Get("Set_CatalogBody");

        /// <summary>Verzeichnis öffnen</summary>
        public static string Set_CatalogOpen => Get("Set_CatalogOpen");

        /// <summary>Export</summary>
        public static string Set_ExportTitle => Get("Set_ExportTitle");

        /// <summary>Tabelle: Werte × Daten (CSV)</summary>
        public static string Set_ExportMatrix => Get("Set_ExportMatrix");

        /// <summary>Liste: eine Zeile je Messung (CSV)</summary>
        public static string Set_ExportFlat => Get("Set_ExportFlat");

        /// <summary>Sicherungskopie (JSON)</summary>
        public static string Set_ExportBackup => Get("Set_ExportBackup");

        /// <summary>Aus Sicherungskopie wiederherstellen</summary>
        public static string Set_ImportBackup => Get("Set_ImportBackup");

        /// <summary>Die Datei entsteht auf dem Gerät; wohin sie geht, entscheiden Sie selb</summary>
        public static string Set_ExportNote => Get("Set_ExportNote");

        /// <summary>KI-Assistent</summary>
        public static string Set_AiTitle => Get("Set_AiTitle");

        /// <summary>Die App funktioniert vollständig ohne KI: Alle Werte lassen sich von H</summary>
        public static string Set_AiBody => Get("Set_AiBody");

        /// <summary>Manueller Weg – funktioniert sofort und braucht keine Berechtigung: Di</summary>
        public static string Set_AiManual => Get("Set_AiManual");

        /// <summary>Tabelle an KI-Chat senden</summary>
        public static string Set_AiShare => Get("Set_AiShare");

        /// <summary>Tabelle in die Zwischenablage</summary>
        public static string Set_AiCopy => Get("Set_AiCopy");

        /// <summary>Befund erkennen</summary>
        public static string Set_AiScan => Get("Set_AiScan");

        /// <summary>Foto oder PDF geht erst beim Antippen von «Erkennen» an den Anbieter.</summary>
        public static string Set_AiScanNote => Get("Set_AiScanNote");

        /// <summary>Erläuterungen zum Verlauf</summary>
        public static string Set_AiComment => Get("Set_AiComment");

        /// <summary>Übertragen werden nur die ausgewählten Werte, ohne Namen und weitere D</summary>
        public static string Set_AiCommentNote => Get("Set_AiCommentNote");

        /// <summary>Alles aus: Die App sendet kein einziges Byte nach außen.</summary>
        public static string Set_AiOff => Get("Set_AiOff");

        /// <summary>Berechtigungen erteilt für «{0}»{1} Ein Anbieter ist in diesem Build n</summary>
        public static string Set_AiGranted => Get("Set_AiGranted");

        /// <summary>Gefahrenzone</summary>
        public static string Set_DangerTitle => Get("Set_DangerTitle");

        /// <summary>Gesamten Verlauf löschen</summary>
        public static string Set_DeleteAll => Get("Set_DeleteAll");

        /// <summary>Die App führt ein persönliches Wertetagebuch und stellt keine Diagnose</summary>
        public static string Set_Disclaimer => Get("Set_Disclaimer");

        /// <summary>Kopiert</summary>
        public static string Set_CopiedTitle => Get("Set_CopiedTitle");

        /// <summary>Die Tabelle liegt in der Zwischenablage – fügen Sie sie in einen belie</summary>
        public static string Set_CopiedBody => Get("Set_CopiedBody");

        /// <summary>Sicherungsdatei wählen</summary>
        public static string Set_ImportPick => Get("Set_ImportPick");

        /// <summary>Import abgeschlossen</summary>
        public static string Set_ImportDoneTitle => Get("Set_ImportDoneTitle");

        /// <summary>Es wurden keine neuen Befunde gefunden.</summary>
        public static string Set_ImportNothing => Get("Set_ImportNothing");

        /// <summary>Hinzugefügte Befunde: {0}.</summary>
        public static string Set_ImportCount => Get("Set_ImportCount");

        /// <summary>Gesamten Verlauf löschen?</summary>
        public static string Set_DeleteAllTitle => Get("Set_DeleteAllTitle");

        /// <summary>Alle Befunde werden vom Gerät gelöscht. Das lässt sich nicht rückgängi</summary>
        public static string Set_DeleteAllBody => Get("Set_DeleteAllBody");

        /// <summary>Daten konnten nicht geladen werden</summary>
        public static string Err_Load => Get("Err_Load");

        /// <summary>Daten konnten nicht aktualisiert werden</summary>
        public static string Err_Refresh => Get("Err_Refresh");

        /// <summary>Tabelle konnte nicht exportiert werden</summary>
        public static string Err_Export => Get("Err_Export");

        /// <summary>Liste konnte nicht exportiert werden</summary>
        public static string Err_ExportList => Get("Err_ExportList");

        /// <summary>Sicherungskopie konnte nicht erstellt werden</summary>
        public static string Err_Backup => Get("Err_Backup");

        /// <summary>Sicherungskopie konnte nicht eingelesen werden</summary>
        public static string Err_Import => Get("Err_Import");

        /// <summary>Verlauf konnte nicht gelöscht werden</summary>
        public static string Err_DeleteAll => Get("Err_DeleteAll");

        /// <summary>Text konnte nicht vorbereitet werden</summary>
        public static string Err_Text => Get("Err_Text");

        /// <summary>Text konnte nicht kopiert werden</summary>
        public static string Err_Copy => Get("Err_Copy");

        /// <summary>Verlauf konnte nicht geladen werden</summary>
        public static string Err_History => Get("Err_History");

        /// <summary>Diagramme konnten nicht erstellt werden</summary>
        public static string Err_Charts => Get("Err_Charts");

        /// <summary>Diagramm konnte nicht erstellt werden</summary>
        public static string Err_Chart => Get("Err_Chart");

        /// <summary>Befund konnte nicht geöffnet werden</summary>
        public static string Err_OpenTest => Get("Err_OpenTest");

        /// <summary>Befund konnte nicht gespeichert werden</summary>
        public static string Err_SaveTest => Get("Err_SaveTest");

        /// <summary>Befund konnte nicht gelöscht werden</summary>
        public static string Err_DeleteTest => Get("Err_DeleteTest");

        /// <summary>Werte konnten nicht übernommen werden</summary>
        public static string Err_CopyRows => Get("Err_CopyRows");

        /// <summary>Text konnte nicht ausgewertet werden</summary>
        public static string Err_Parse => Get("Err_Parse");

        /// <summary>Anfrage konnte nicht kopiert werden</summary>
        public static string Err_Prompt => Get("Err_Prompt");

        /// <summary>Einstellungen konnten nicht geöffnet werden</summary>
        public static string Err_Settings => Get("Err_Settings");

        /// <summary>Verzeichnis konnte nicht geöffnet werden</summary>
        public static string Err_Catalog => Get("Err_Catalog");

        /// <summary>Wert konnte nicht geöffnet werden</summary>
        public static string Err_CatalogItem => Get("Err_CatalogItem");

        /// <summary>Wert konnte nicht gespeichert werden</summary>
        public static string Err_CatalogSave => Get("Err_CatalogSave");

        /// <summary>Wert konnte nicht gelöscht werden</summary>
        public static string Err_CatalogDelete => Get("Err_CatalogDelete");

        /// <summary>Werte konnten nicht zusammengeführt werden</summary>
        public static string Err_Merge => Get("Err_Merge");

        /// <summary>Favorit konnte nicht geändert werden</summary>
        public static string Err_Favorite => Get("Err_Favorite");

        /// <summary>Laborwerte</summary>
        public static string Share_Results => Get("Share_Results");

        /// <summary>Laborwerte (Tabelle)</summary>
        public static string Share_ResultsTable => Get("Share_ResultsTable");

        /// <summary>Laborwerte (Liste)</summary>
        public static string Share_ResultsList => Get("Share_ResultsList");

        /// <summary>Sicherungskopie des Verlaufs</summary>
        public static string Share_Backup => Get("Share_Backup");

        /// <summary>Wert</summary>
        public static string Csv_Parameter => Get("Csv_Parameter");

        /// <summary>Einheit</summary>
        public static string Csv_Unit => Get("Csv_Unit");

        /// <summary>Referenzbereich</summary>
        public static string Csv_Reference => Get("Csv_Reference");

        /// <summary>Datum</summary>
        public static string Csv_Date => Get("Csv_Date");

        /// <summary>Labor</summary>
        public static string Csv_Lab => Get("Csv_Lab");

        /// <summary>Code</summary>
        public static string Csv_Code => Get("Csv_Code");

        /// <summary>Ergebnis</summary>
        public static string Csv_Value => Get("Csv_Value");

        /// <summary>von</summary>
        public static string Csv_Min => Get("Csv_Min");

        /// <summary>bis</summary>
        public static string Csv_Max => Get("Csv_Max");

        /// <summary>Status</summary>
        public static string Csv_Status => Get("Csv_Status");

        /// <summary>Kommentar</summary>
        public static string Csv_Comment => Get("Csv_Comment");

        /// <summary>unter Referenz</summary>
        public static string Csv_StatusLow => Get("Csv_StatusLow");

        /// <summary>über Referenz</summary>
        public static string Csv_StatusHigh => Get("Csv_StatusHigh");

        /// <summary>im Referenzbereich</summary>
        public static string Csv_StatusNormal => Get("Csv_StatusNormal");

        /// <summary>Tagebuch der Laborwerte. Die Werte wurden von Hand aus Laborbefunden ü</summary>
        public static string Txt_Header => Get("Txt_Header");

        /// <summary>Personenbezogene Daten sind hier nicht enthalten – nur Werte, Einheite</summary>
        public static string Txt_NoPersonal => Get("Txt_NoPersonal");

        /// <summary>Referenzbereiche unterscheiden sich je Labor; angegeben sind die aus d</summary>
        public static string Txt_RefNote => Get("Txt_RefNote");

        /// <summary>Letzter Befund: {0}.</summary>
        public static string Txt_Latest => Get("Txt_Latest");

        /// <summary>Im letzten Befund liegen alle Werte im Referenzbereich.</summary>
        public static string Txt_AllInRange => Get("Txt_AllInRange");

        /// <summary>Außerhalb des Referenzbereichs im letzten Befund: {0}.</summary>
        public static string Txt_OutOfRange => Get("Txt_OutOfRange");

        /// <summary>Es sind noch keine Befunde erfasst.</summary>
        public static string Txt_Empty => Get("Txt_Empty");

        /// <summary>nicht angebunden</summary>
        public static string Ai_NotConnected => Get("Ai_NotConnected");

        /// <summary>Der Text ist leer.</summary>
        public static string Imp_EmptyText => Get("Imp_EmptyText");

        /// <summary>Datum nicht erkannt: «{0}».</summary>
        public static string Imp_BadDate => Get("Imp_BadDate");

        /// <summary>Zeile übersprungen: «{0}».</summary>
        public static string Imp_SkippedLine => Get("Imp_SkippedLine");

        /// <summary>Es wurden keine Wertezeilen gefunden.</summary>
        public static string Imp_NoRows => Get("Imp_NoRows");

        /// <summary>Sprache</summary>
        public static string Set_LanguageTitle => Get("Set_LanguageTitle");

        /// <summary>«System» übernimmt die Sprache des Geräts. Die Umstellung wirkt sofort</summary>
        public static string Set_LanguageNote => Get("Set_LanguageNote");

        /// <summary>Blutbild</summary>
        public static string Cat_Group_Cbc => Get("Cat_Group_Cbc");

        /// <summary>Leberwerte</summary>
        public static string Cat_Group_Liver => Get("Cat_Group_Liver");

        /// <summary>Nierenwerte</summary>
        public static string Cat_Group_Kidney => Get("Cat_Group_Kidney");

        /// <summary>Blutfette</summary>
        public static string Cat_Group_Lipids => Get("Cat_Group_Lipids");

        /// <summary>Stoffwechsel</summary>
        public static string Cat_Group_Metabolism => Get("Cat_Group_Metabolism");

        /// <summary>Eisenstoffwechsel</summary>
        public static string Cat_Group_Iron => Get("Cat_Group_Iron");

        /// <summary>Vitamine</summary>
        public static string Cat_Group_Vitamins => Get("Cat_Group_Vitamins");

        /// <summary>Elektrolyte</summary>
        public static string Cat_Group_Electrolytes => Get("Cat_Group_Electrolytes");

        /// <summary>Schilddrüse</summary>
        public static string Cat_Group_Thyroid => Get("Cat_Group_Thyroid");

        /// <summary>Hormone</summary>
        public static string Cat_Group_Hormones => Get("Cat_Group_Hormones");

        /// <summary>Entzündung</summary>
        public static string Cat_Group_Inflammation => Get("Cat_Group_Inflammation");

        /// <summary>Meine Werte</summary>
        public static string Cat_Group_Own => Get("Cat_Group_Own");

        /// <summary>Systemsprache</summary>
        public static string Lang_System => Get("Lang_System");

        /// <summary>Tabelle</summary>
        public static string Tab_Matrix => Get("Tab_Matrix");

        /// <summary>Die Tabelle füllt sich, sobald der erste Befund erfasst ist. Zeilen si</summary>
        public static string Matrix_Empty => Get("Matrix_Empty");

        /// <summary>Alle Werte</summary>
        public static string Matrix_AllValues => Get("Matrix_AllValues");

        /// <summary>Neue Ansicht</summary>
        public static string ViewEdit_TitleNew => Get("ViewEdit_TitleNew");

        /// <summary>Name der Ansicht</summary>
        public static string ViewEdit_Name => Get("ViewEdit_Name");

        /// <summary>z. B. Eisen &amp; Blutbild</summary>
        public static string ViewEdit_NamePlaceholder => Get("ViewEdit_NamePlaceholder");

        /// <summary>Noch nichts ausgewählt. Tippen Sie die Werte an, die in die Ansicht so</summary>
        public static string ViewEdit_NothingPicked => Get("ViewEdit_NothingPicked");

        /// <summary>1 Wert ausgewählt</summary>
        public static string ViewEdit_OnePicked => Get("ViewEdit_OnePicked");

        /// <summary>{0} Werte ausgewählt · Reihenfolge wie ausgewählt</summary>
        public static string ViewEdit_ManyPicked => Get("ViewEdit_ManyPicked");

        /// <summary>Name fehlt</summary>
        public static string ViewEdit_NoNameTitle => Get("ViewEdit_NoNameTitle");

        /// <summary>Geben Sie der Ansicht einen Namen.</summary>
        public static string ViewEdit_NoNameBody => Get("ViewEdit_NoNameBody");

        /// <summary>Nichts ausgewählt</summary>
        public static string ViewEdit_EmptyTitle => Get("ViewEdit_EmptyTitle");

        /// <summary>Wählen Sie mindestens einen Wert aus.</summary>
        public static string ViewEdit_EmptyBody => Get("ViewEdit_EmptyBody");

        /// <summary>Ansicht löschen?</summary>
        public static string ViewEdit_DeleteTitle => Get("ViewEdit_DeleteTitle");

        /// <summary>Nur die Ansicht wird gelöscht. Die Messungen bleiben unberührt.</summary>
        public static string ViewEdit_DeleteBody => Get("ViewEdit_DeleteBody");

        /// <summary>Ansicht konnte nicht geöffnet werden</summary>
        public static string Err_View => Get("Err_View");

        /// <summary>Ansicht konnte nicht gespeichert werden</summary>
        public static string Err_ViewSave => Get("Err_ViewSave");

        /// <summary>Ansicht konnte nicht gelöscht werden</summary>
        public static string Err_ViewDelete => Get("Err_ViewDelete");

        /// <summary>Für diesen Tag gibt es schon Werte</summary>
        public static string Edit_MergeTitle => Get("Edit_MergeTitle");

        /// <summary>Zum {0} sind diese Werte bereits erfasst – mit einem anderen Ergebnis:</summary>
        public static string Edit_MergeBody => Get("Edit_MergeBody");

        /// <summary>Ersetzen</summary>
        public static string Edit_MergeReplace => Get("Edit_MergeReplace");

        /// <summary>Bisherige behalten</summary>
        public static string Edit_MergeKeep => Get("Edit_MergeKeep");

        /// <summary>… und {0} weitere</summary>
        public static string Edit_MergeMore => Get("Edit_MergeMore");

        /// <summary>Andere Einheit als bisher: {0}. Werte in verschiedenen Einheiten sind </summary>
        public static string Edit_OtherUnits => Get("Edit_OtherUnits");

        /// <summary>Über die App</summary>
        public static string Set_AboutTitle => Get("Set_AboutTitle");

        /// <summary>Die Version entspricht dem Release auf GitHub. Alle Daten bleiben auf </summary>
        public static string Set_AboutBody => Get("Set_AboutBody");

        /// <summary>Version {0} (Build {1})</summary>
        public static string Set_Version => Get("Set_Version");
    }
}
