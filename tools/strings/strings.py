# -*- coding: utf-8 -*-
"""
Таблица текстов интерфейса: (ключ, немецкий, русский).

Это исходник — AppResources.resx, AppResources.ru.resx и S.cs собираются из него
командой `python3 tools/strings/gen.py`. Править файлы в Resources/Strings руками
бессмысленно: следующая генерация всё вернёт. Так оба языка лежат в одной строке
и перевод невозможно забыть — новый текст физически не добавить без пары.

Немецкий — язык по умолчанию (NeutralLanguage=de), поэтому он идёт в основной resx.
"""
S = [
# --- allgemein ---
("App_Title","Laborwerte","Мои анализы"),
("Tab_Dashboard","Übersicht","Обзор"),
("Tab_History","Befunde","История"),
("Tab_Trends","Verlauf","Динамика"),
("Tab_Settings","Einstellungen","Настройки"),
("Common_Ok","OK","OK"),
("Common_Cancel","Abbrechen","Отмена"),
("Common_Delete","Löschen","Удалить"),
("Common_Save","Speichern","Сохранить"),
("Common_Error","Fehler","Ошибка"),
("Common_None","—","—"),
# --- Übersicht ---
("Dash_Title","Meine Laborwerte","Мои анализы"),
("Dash_NoTests","Noch kein Befund erfasst","Пока нет ни одного анализа"),
("Dash_LastTest","Letzter Befund: {0}","Последний анализ: {0}"),
("Dash_EmptyHint","Erfassen Sie den ersten Befund – den Verlauf zeigt die App dann von selbst.","Добавьте первый анализ — дальше приложение само покажет динамику."),
("Dash_StartTitle","So fangen Sie an","С чего начать"),
("Dash_StartBody","Nehmen Sie Ihren Laborbefund und tragen Sie die Werte ein: Bezeichnung, Zahl, Einheit und den Referenzbereich aus der entsprechenden Spalte.","Возьмите бланк анализа и внесите значения — название, число, единицы и норму из колонки «референсные значения»."),
("Dash_StartNote","Alles wird lokal auf diesem Gerät gespeichert. Es werden keine Daten übertragen.","Всё сохраняется в локальной базе на этом устройстве. Никуда ничего не отправляется."),
("Dash_OutOfRange","Außerhalb des Referenzbereichs","Вне нормы"),
("Dash_Changes","Deutliche Veränderungen","Заметные изменения"),
("Dash_Summary","{0} Werte · {1} · {2}","{0} показателей · {1} · {2}"),
("Dash_AllInRange","alle Werte im Referenzbereich","все показатели в пределах указанных норм"),
("Dash_OneOut","1 Wert außerhalb","1 показатель вне нормы"),
("Dash_ManyOut","{0} Werte außerhalb","{0} показателей вне нормы"),
("Dash_OneTest","1 Befund im Verlauf","1 анализ в истории"),
("Dash_ManyTests","{0} Befunde im Verlauf","{0} анализов в истории"),
# --- Befunde ---
("Hist_Empty","Hier erscheinen alle gespeicherten Befunde – einer pro Blutabnahme.","Здесь появятся все сохранённые анализы — по одному на каждую сдачу."),
("Hist_Add","➕ Befund erfassen","➕ Внести анализ"),
("Hist_Export","⬇ Tabelle exportieren","⬇ Выгрузить таблицу"),
("Hist_SubtitleOk","{0} Werte · alle im Referenzbereich","{0} показателей · всё в пределах норм"),
("Hist_SubtitleOut","{0} Werte · {1} außerhalb","{0} показателей · {1} вне нормы"),
# --- Befund erfassen ---
("Edit_TitleNew","Neuer Befund","Новый анализ"),
("Edit_TitleExisting","Befund","Анализ"),
("Edit_Date","Datum der Abnahme","Дата сдачи"),
("Edit_Lab","Labor (optional)","Лаборатория (необязательно)"),
("Edit_LabPlaceholder","z. B. Synlab","Например, Инвитро"),
("Edit_Note","Notiz (optional)","Заметка (необязательно)"),
("Edit_NotePlaceholder","nüchtern, nach Vitaminkur …","Натощак, после курса витаминов…"),
("Edit_AddParam","Wert hinzufügen","Добавить показатель"),
("Edit_SearchPlaceholder","Bezeichnung eingeben","Начните вводить название"),
("Edit_AddTyped","So übernehmen","Добавить как есть"),
("Edit_AddEmpty","Leere Zeile","Пустая строка"),
("Edit_CopyPrevious","Wie beim letzten Mal","Как в прошлый раз"),
("Edit_PhotoTitle","Aus einem Foto ausfüllen","Заполнить из фотографии"),
("Edit_PhotoBody","Den Befund liest Ihr KI-Chat, nicht die App. Anfrage kopieren, zusammen mit dem Foto in den Chat schicken, Antwort kopieren – und hier einfügen.","Распознаёт бланк ваш чат-бот, а не приложение. Скопируйте запрос, отправьте его в чат вместе с фотографией, ответ скопируйте — и вставьте сюда."),
("Edit_CopyPrompt","1. Anfrage kopieren","1. Скопировать запрос"),
("Edit_Paste","2. Aus Zwischenablage","2. Вставить из буфера"),
("Edit_PhotoNote","Die App sendet dabei nichts: Der Austausch mit dem Chat läuft von Hand über die Zwischenablage.","Приложение при этом никуда ничего не отправляет: обмен с чатом идёт через буфер обмена, вручную."),
("Edit_RowName","Bezeichnung","Название показателя"),
("Edit_RowRefMin","Referenz von","Норма от"),
("Edit_RowRefMax","Referenz bis","Норма до"),
("Edit_Hint","Zahlen dürfen mit Komma oder Punkt geschrieben werden. Den Referenzbereich übernehmen Sie aus Ihrem Befund – jedes Labor hat eigene Grenzen.","Значения можно вводить и через точку, и через запятую. Норму берите из колонки «референсные значения» своего бланка — у каждой лаборатории она своя."),
("Edit_NoPrevTitle","Keine Daten","Нет данных"),
("Edit_NoPrevBody","Es gibt keinen früheren Befund.","Более раннего анализа в истории нет."),
("Edit_EmptyTitle","Leer","Пусто"),
("Edit_EmptyBody","Fügen Sie mindestens einen Wert hinzu.","Добавьте хотя бы один показатель."),
("Edit_NoNameTitle","Bezeichnung fehlt","Не хватает названия"),
("Edit_NoNameBody","Jede ausgefüllte Zeile braucht eine Bezeichnung.","У каждой заполненной строки должно быть название показателя."),
("Edit_DeleteTitle","Befund löschen?","Удалить анализ?"),
("Edit_DeleteBody","Der Eintrag wird unwiderruflich vom Gerät gelöscht.","Запись будет удалена с устройства безвозвратно."),
("Edit_PromptCopiedTitle","Anfrage kopiert","Запрос скопирован"),
("Edit_PromptCopiedBody","Fügen Sie sie zusammen mit dem Foto des Befunds in einen beliebigen KI-Chat ein. Die Antwort kopieren und hier auf «Aus Zwischenablage» tippen.","Вставьте его в любой чат-бот вместе с фотографией бланка. Полученный ответ скопируйте и вернитесь сюда — кнопка «Вставить из буфера» разложит его по строкам."),
("Edit_ClipEmptyTitle","Zwischenablage ist leer","Буфер пуст"),
("Edit_ClipEmptyBody","Kopieren Sie zuerst die Tabelle mit den Werten.","Сначала скопируйте таблицу с результатами."),
("Edit_NoRowsTitle","Nichts erkannt","Не разобрано"),
("Edit_NoRowsBody","Im Text wurden keine Wertezeilen gefunden. Eine Zeile sollte so aussehen:\n\nFerritin | 18 | ng/ml | 30 | 300","В тексте не нашлось строк показателей. Каждая строка должна выглядеть так:\n\nФерритин | 18 | мкг/л | 30 | 300"),
("Edit_Added","{0} Zeilen übernommen. Bitte vor dem Speichern prüfen.","Добавлено строк: {0}. Проверьте значения перед сохранением."),
("Edit_AddedWarn","{0} Nicht erkannt: {1}","{0} Не разобрано: {1}"),
# --- Verlauf ---
("Trend_Empty","Diagramme erscheinen, sobald zwei Befunde mit denselben Werten vorliegen.","Графики появятся, когда в истории будет хотя бы два анализа с одинаковыми показателями."),
("Trend_OnlyFavorites","★ Nur Favoriten","★ Только избранное"),
("Trend_FavoritesGroup","★ Favoriten","★ Избранное"),
("Trend_NoGroup","Ohne Gruppe","Без группы"),
("Trend_OneMeasurement","eine Messung","одно измерение"),
("Trend_NoChange","→ unverändert","→ без изменений"),
("Trend_Values","Werte","Значения"),
("Trend_RefKnown","Referenz: {0} {1}","Норма: {0} {1}"),
("Trend_RefUnknown","Kein Referenzbereich angegeben","Норма не указана"),
("Trend_SinglePoint","Eine Messung – der Verlauf entsteht ab dem nächsten Befund.","Одно измерение — динамика появится после следующего анализа."),
("Trend_Since","{0} Messungen seit {1}","{0} измерений с {1}"),
("Trend_NoData","Zu diesem Wert liegen keine Daten vor.","Данных по этому показателю нет."),
("Trend_ChartNoData","Keine Daten","Нет данных"),
# --- Status ---
("Status_Low","unter dem Referenzbereich","ниже нормы"),
("Status_High","über dem Referenzbereich","выше нормы"),
("Status_Normal","im Referenzbereich","в норме"),
("Status_Unknown","kein Referenzbereich","норма не задана"),
("Assess_Improved","besser als beim letzten Mal","лучше, чем в прошлый раз"),
("Assess_Worsened","schlechter als beim letzten Mal","хуже, чем в прошлый раз"),
("Assess_Stable","unverändert","без изменений"),
("Assess_Unknown","kein Vergleich möglich","не с чем сравнить"),
("Item_RefKnown","Referenz {0}","норма {0}"),
("Item_RefUnknown","kein Referenzbereich","норма не указана"),
("Item_Previous","zuvor {0} · {1}","было {0} · {1}"),
("Item_First","erste Messung","первое измерение"),
# --- Werteverzeichnis ---
("Cat_Title","Werteverzeichnis","Справочник"),
("Cat_Search","Suche nach Bezeichnung","Поиск по названию"),
("Cat_OnlyFavorites","Nur Favoriten","Только избранное"),
("Cat_ShowHidden","Ausgeblendete zeigen","Показать скрытые"),
("Cat_AddOwn","➕ Eigener Wert","➕ Свой показатель"),
("Cat_Hidden","aus den Vorschlägen ausgeblendet","скрыт из подсказок"),
("Cat_Summary","{0} Werte · {1} als Favorit","{0} показателей · в избранном {1}"),
("Cat_SummaryHidden","{0} Werte · {1} als Favorit · {2} ausgeblendet","{0} показателей · в избранном {1} · скрыто {2}"),
("Cat_Unused","nicht verwendet","не использовался"),
("Cat_OneMeasurement","1 Messung","1 измерение"),
("Cat_ManyMeasurements","{0} Messungen","{0} измерений"),
("Cat_BuiltIn","vorgegeben","встроенный"),
("Cat_Own","eigener","свой"),
("Cat_NoRef","kein Referenzbereich","норма не задана"),
("Cat_OneParam","1 Wert","1 показатель"),
("Cat_ManyParams","{0} Werte","{0} показателей"),
# --- Wert bearbeiten ---
("CatEdit_TitleNew","Neuer Wert","Новый показатель"),
("CatEdit_Name","Bezeichnung","Название"),
("CatEdit_NamePlaceholder","z. B. Homocystein","Например, Гомоцистеин"),
("CatEdit_UnitPlaceholder","µmol/l","мкмоль/л"),
("CatEdit_Group","Gruppe","Группа"),
("CatEdit_GroupPick","Vorhandene wählen","Выбрать существующую"),
("CatEdit_GroupNew","… oder neue eingeben","…или впишите новую"),
("CatEdit_Notes","Hinweis zum Referenzbereich","Пояснение к норме"),
("CatEdit_NotesPlaceholder","z. B. Bereich für Männer","Например: диапазон для мужчин"),
("CatEdit_Note","Diese Angaben werden beim Erfassen nur vorgeschlagen. In gespeicherten Messungen bleiben die Grenzen aus Ihrem Befund stehen.","Эти значения только подставляются при вводе анализа. В сохранённых измерениях остаются те границы, что были на бланке."),
("CatEdit_Code","Code: {0}","Код: {0}"),
("CatEdit_CodeNew","Der Code entsteht aus der Bezeichnung","Код будет создан из названия"),
("CatEdit_Favorite","★ Als Favorit","★ В избранном"),
("CatEdit_FavoriteNote","Der Wert erscheint oben auf der Startseite und in einer eigenen Gruppe im Verlauf.","Показатель попадёт наверх главного экрана и в отдельную группу в динамике."),
("CatEdit_Hide","Aus Vorschlägen ausblenden","Скрыть из подсказок"),
("CatEdit_HideNote","Der Eintrag bleibt erhalten, wird beim Erfassen aber nicht mehr vorgeschlagen.","Запись останется, но перестанет предлагаться при вводе."),
("CatEdit_MergeTitle","Mit anderem Wert zusammenführen","Объединить с другим"),
("CatEdit_MergeBody","Steht derselbe Wert doppelt im Verzeichnis, verschieben Sie seine Messungen zum Haupteintrag – dann ist es im Diagramm wieder eine Linie.","Если один и тот же показатель попал в справочник дважды — перенесите его измерения к основной записи, и на графике снова будет одна линия."),
("CatEdit_MergeTarget","Wohin verschieben","Куда перенести"),
("CatEdit_Merge","Zusammenführen","Объединить"),
("CatEdit_NoUsage","Zu diesem Wert gibt es noch keine Messungen.","Измерений с этим показателем пока нет."),
("CatEdit_OneUsage","1 gespeicherte Messung.","1 сохранённое измерение."),
("CatEdit_ManyUsage","{0} gespeicherte Messungen.","{0} сохранённых измерений."),
("CatEdit_NoNameTitle","Bezeichnung fehlt","Не хватает названия"),
("CatEdit_NoNameBody","Geben Sie eine Bezeichnung an.","Укажите название показателя."),
("CatEdit_ExistsTitle","Gibt es bereits","Такой показатель уже есть"),
("CatEdit_ExistsBody","Ein Wert mit dieser Bezeichnung steht schon im Verzeichnis. Öffnen Sie ihn und bearbeiten Sie ihn dort.","Показатель с таким названием уже в справочнике. Откройте его и отредактируйте."),
("CatEdit_BuiltInTitle","Vorgegebener Wert","Встроенный показатель"),
("CatEdit_BuiltInBody","Vorgegebene Werte lassen sich nicht löschen – sie kehren beim nächsten Start zurück. Blenden Sie ihn stattdessen mit dem Schalter aus.","Встроенные показатели не удаляются — при следующем запуске они вернутся. Уберите его из подсказок переключателем «Скрыть»."),
("CatEdit_DeleteTitle","Aus dem Verzeichnis löschen?","Удалить из справочника?"),
("CatEdit_DeleteBody","Der Verzeichniseintrag wird gelöscht.","Запись справочника будет удалена."),
("CatEdit_DeleteBodyUsed","Der Verzeichniseintrag wird gelöscht. Die gespeicherten Messungen ({0}) bleiben erhalten und ergeben weiterhin ein Diagramm – das Verzeichnis schlägt beim Erfassen nur Werte vor.","Запись справочника будет удалена. Сохранённые измерения ({0}) останутся на месте и продолжат собираться в один график — справочник лишь подставляет значения при вводе."),
("CatEdit_MergePickTitle","Wert wählen","Выберите показатель"),
("CatEdit_MergePickBody","Geben Sie an, mit welchem Wert zusammengeführt werden soll.","Укажите, с каким показателем объединить этот."),
("CatEdit_MergeConfirmTitle","Werte zusammenführen?","Объединить показатели?"),
("CatEdit_MergeConfirmBody","Die Messungen von «{0}» ({1}) gehen an «{2}» über und bilden künftig eine Linie im Diagramm. Rückgängig macht das nur ein erneutes Zusammenführen in die andere Richtung.","Измерения «{0}» ({1}) перейдут к «{2}» и дальше будут одной линией на графике. Отменить это можно только повторным объединением в обратную сторону."),
("CatEdit_MergeDoneTitle","Fertig","Готово"),
("CatEdit_MergeDoneBody","Verschobene Messungen: {0}.","Перенесено измерений: {0}."),
# --- Einstellungen ---
("Set_StorageTitle","Speicherung","Хранение"),
("Set_StorageEmpty","Noch keine Befunde. Die Daten liegen ausschließlich auf diesem Gerät.","История пуста. Данные хранятся только на этом устройстве."),
("Set_StorageOne","1 Befund. Die Daten liegen ausschließlich auf diesem Gerät.","1 анализ. Данные хранятся только на этом устройстве."),
("Set_StorageMany","{0} Befunde. Die Daten liegen ausschließlich auf diesem Gerät.","{0} анализов. Данные хранятся только на этом устройстве."),
("Set_DbFile","Datenbankdatei:","Файл базы:"),
("Set_CatalogTitle","Werteverzeichnis","Справочник показателей"),
("Set_CatalogBody","Was beim Erfassen vorgeschlagen wird: Bezeichnungen, Einheiten und übliche Referenzbereiche. Hier werden auch Doppelte zusammengeführt und Überflüssiges ausgeblendet.","Что подставляется при вводе анализа: названия, единицы и типовые нормы. Здесь же объединяются дубли и прячется лишнее."),
("Set_CatalogOpen","Verzeichnis öffnen","Открыть справочник"),
("Set_ExportTitle","Export","Выгрузка"),
("Set_ExportMatrix","Tabelle: Werte × Daten (CSV)","Таблица: показатели × даты (CSV)"),
("Set_ExportFlat","Liste: eine Zeile je Messung (CSV)","Список: одна строка на измерение (CSV)"),
("Set_ExportBackup","Sicherungskopie (JSON)","Резервная копия (JSON)"),
("Set_ImportBackup","Aus Sicherungskopie wiederherstellen","Восстановить из копии"),
("Set_ExportNote","Die Datei entsteht auf dem Gerät; wohin sie geht, entscheiden Sie selbst.","Файл создаётся на устройстве, а дальше вы сами решаете, куда его отправить."),
("Set_AiTitle","KI-Assistent","ИИ-помощник"),
("Set_AiBody","Die App funktioniert vollständig ohne KI: Alle Werte lassen sich von Hand eintragen. Der Assistent ist eine freiwillige Ergänzung, und jede Berechtigung wird einzeln erteilt.","Приложение полностью работает без ИИ: все значения можно ввести руками. Помощник — необязательная надстройка, и каждое разрешение выдаётся отдельно."),
("Set_AiManual","Manueller Weg – funktioniert sofort und braucht keine Berechtigung: Die App bereitet die Tabelle als Text auf, den Empfänger wählen Sie selbst.","Ручная передача — работает уже сейчас и не требует никаких разрешений: приложение готовит таблицу текстом, а получателя вы выбираете сами."),
("Set_AiShare","Tabelle an KI-Chat senden","Отправить таблицу в ИИ-чат"),
("Set_AiCopy","Tabelle in die Zwischenablage","Скопировать таблицу в буфер"),
("Set_AiOff","Alles aus: Die App sendet kein einziges Byte nach außen.","Всё выключено: приложение не отправляет наружу ни одного байта."),
("Set_AiGranted","Berechtigungen erteilt für «{0}»{1} Ein Anbieter ist in diesem Build noch nicht angebunden, es findet also keine Übertragung statt.","Разрешения выданы для «{0}»{1} Провайдер в этой сборке ещё не подключён, поэтому фактической отправки не происходит."),
("Set_DangerTitle","Gefahrenzone","Опасная зона"),
("Set_DeleteAll","Gesamten Verlauf löschen","Удалить всю историю"),
("Set_Disclaimer","Die App führt ein persönliches Wertetagebuch und stellt keine Diagnosen. Besprechen Sie Schlussfolgerungen mit Ihrer Ärztin oder Ihrem Arzt.","Приложение ведёт личный журнал результатов и не ставит диагнозов. Любые выводы обсуждайте с врачом."),
("Set_CopiedTitle","Kopiert","Скопировано"),
("Set_CopiedBody","Die Tabelle liegt in der Zwischenablage – fügen Sie sie in einen beliebigen Chat ein.","Таблица в буфере обмена — вставьте её в любой чат."),
("Set_ImportPick","Sicherungsdatei wählen","Выберите файл резервной копии"),
("Set_ImportDoneTitle","Import abgeschlossen","Импорт завершён"),
("Set_ImportNothing","Es wurden keine neuen Befunde gefunden.","Новых анализов в файле не найдено."),
("Set_ImportCount","Hinzugefügte Befunde: {0}.","Добавлено анализов: {0}."),
("Set_DeleteAllTitle","Gesamten Verlauf löschen?","Удалить всю историю?"),
("Set_DeleteAllBody","Alle Befunde werden vom Gerät gelöscht. Das lässt sich nicht rückgängig machen – legen Sie vorher eine Sicherungskopie an.","Все анализы будут удалены с устройства. Действие необратимо — сначала сделайте резервную копию."),
# --- Fehlermeldungen ---
("Err_Load","Daten konnten nicht geladen werden","Не удалось загрузить данные"),
("Err_Refresh","Daten konnten nicht aktualisiert werden","Не удалось обновить данные"),
("Err_Export","Tabelle konnte nicht exportiert werden","Не удалось выгрузить таблицу"),
("Err_ExportList","Liste konnte nicht exportiert werden","Не удалось выгрузить список"),
("Err_Backup","Sicherungskopie konnte nicht erstellt werden","Не удалось создать резервную копию"),
("Err_Import","Sicherungskopie konnte nicht eingelesen werden","Не удалось импортировать резервную копию"),
("Err_DeleteAll","Verlauf konnte nicht gelöscht werden","Не удалось очистить историю"),
("Err_Text","Text konnte nicht vorbereitet werden","Не удалось подготовить текст"),
("Err_Copy","Text konnte nicht kopiert werden","Не удалось скопировать текст"),
("Err_History","Verlauf konnte nicht geladen werden","Не удалось загрузить историю"),
("Err_Charts","Diagramme konnten nicht erstellt werden","Не удалось построить графики"),
("Err_Chart","Diagramm konnte nicht erstellt werden","Не удалось построить график"),
("Err_OpenTest","Befund konnte nicht geöffnet werden","Не удалось открыть анализ"),
("Err_SaveTest","Befund konnte nicht gespeichert werden","Не удалось сохранить анализ"),
("Err_DeleteTest","Befund konnte nicht gelöscht werden","Не удалось удалить анализ"),
("Err_CopyRows","Werte konnten nicht übernommen werden","Не удалось скопировать показатели"),
("Err_Parse","Text konnte nicht ausgewertet werden","Не удалось разобрать текст"),
("Err_Prompt","Anfrage konnte nicht kopiert werden","Не удалось скопировать запрос"),
("Err_Settings","Einstellungen konnten nicht geöffnet werden","Не удалось открыть настройки"),
("Err_Catalog","Verzeichnis konnte nicht geöffnet werden","Не удалось открыть справочник"),
("Err_CatalogItem","Wert konnte nicht geöffnet werden","Не удалось открыть показатель"),
("Err_CatalogSave","Wert konnte nicht gespeichert werden","Не удалось сохранить показатель"),
("Err_CatalogDelete","Wert konnte nicht gelöscht werden","Не удалось удалить показатель"),
("Err_Merge","Werte konnten nicht zusammengeführt werden","Не удалось объединить показатели"),
("Err_Favorite","Favorit konnte nicht geändert werden","Не удалось изменить избранное"),
# --- Export/Teilen ---
("Share_Results","Laborwerte","Результаты анализов"),
("Share_ResultsTable","Laborwerte (Tabelle)","Результаты анализов (таблица)"),
("Share_ResultsList","Laborwerte (Liste)","Результаты анализов (список)"),
("Share_Backup","Sicherungskopie des Verlaufs","Резервная копия истории"),
("Csv_Parameter","Wert","Показатель"),
("Csv_Unit","Einheit","Единицы"),
("Csv_Reference","Referenzbereich","Норма"),
("Csv_Date","Datum","Дата"),
("Csv_Lab","Labor","Лаборатория"),
("Csv_Code","Code","Код"),
("Csv_Value","Ergebnis","Значение"),
("Csv_Min","von","Мин"),
("Csv_Max","bis","Макс"),
("Csv_Status","Status","Статус"),
("Csv_Comment","Kommentar","Комментарий"),
("Csv_StatusLow","unter Referenz","ниже нормы"),
("Csv_StatusHigh","über Referenz","выше нормы"),
("Csv_StatusNormal","im Referenzbereich","норма"),
("Txt_Header","Tagebuch der Laborwerte. Die Werte wurden von Hand aus Laborbefunden übernommen.","Журнал результатов анализов. Значения внесены вручную из бланков лаборатории."),
("Txt_NoPersonal","Personenbezogene Daten sind hier nicht enthalten – nur Werte, Einheiten, Referenzbereiche aus dem Befund und Datumsangaben.","Персональных данных здесь нет — только показатели, единицы, границы норм из бланка и даты."),
("Txt_RefNote","Referenzbereiche unterscheiden sich je Labor; angegeben sind die aus dem jeweiligen Befund.","Границы норм у разных лабораторий отличаются; в таблице приведены те, что были напечатаны в бланке."),
("Txt_Latest","Letzter Befund: {0}.","Последний анализ: {0}."),
("Txt_AllInRange","Im letzten Befund liegen alle Werte im Referenzbereich.","В последнем анализе все показатели в пределах указанных норм."),
("Txt_OutOfRange","Außerhalb des Referenzbereichs im letzten Befund: {0}.","Вне нормы в последнем анализе: {0}."),
("Txt_Empty","Es sind noch keine Befunde erfasst.","История анализов пуста."),
("Ai_NotConnected","nicht angebunden","не подключён"),
]
S += [
("Imp_EmptyText","Der Text ist leer.","Пустой текст."),
("Imp_BadDate","Datum nicht erkannt: «{0}».","Не разобрана дата: «{0}»."),
("Imp_SkippedLine","Zeile übersprungen: «{0}».","Пропущена строка: «{0}»."),
("Imp_NoRows","Es wurden keine Wertezeilen gefunden.","Не найдено ни одной строки показателей."),
]
S += [
("Set_LanguageTitle","Sprache","Язык"),
("Set_LanguageNote","«System» übernimmt die Sprache des Geräts. Die Umstellung wirkt sofort, ohne Neustart.","«System» — язык устройства. Переключение применяется сразу, без перезапуска."),
]
S += [
("Cat_Group_Cbc","Blutbild","Общий анализ крови"),
("Cat_Group_Liver","Leberwerte","Печёночные показатели"),
("Cat_Group_Kidney","Nierenwerte","Почечные показатели"),
("Cat_Group_Lipids","Blutfette","Липиды"),
("Cat_Group_Metabolism","Stoffwechsel","Обмен веществ"),
("Cat_Group_Iron","Eisenstoffwechsel","Обмен железа"),
("Cat_Group_Vitamins","Vitamine","Витамины"),
("Cat_Group_Electrolytes","Elektrolyte","Электролиты"),
("Cat_Group_Thyroid","Schilddrüse","Щитовидная железа"),
("Cat_Group_Hormones","Hormone","Гормоны"),
("Cat_Group_Inflammation","Entzündung","Воспаление"),
("Cat_Group_Own","Meine Werte","Мои показатели"),
("Lang_System","Systemsprache","Язык системы"),
]
S += [
("Tab_Matrix","Tabelle","Таблица"),
("Matrix_Empty","Die Tabelle füllt sich, sobald der erste Befund erfasst ist. Zeilen sind Werte, Spalten die Abnahmedaten – nach rechts geht es weiter.","Таблица заполнится, когда появится первый анализ. Строки — показатели, столбцы — даты сдач, и дальше вправо."),
]
S += [
("Matrix_AllValues","Alle Werte","Все показатели"),
("ViewEdit_TitleNew","Neue Ansicht","Новый набор"),
("ViewEdit_Name","Name der Ansicht","Название набора"),
("ViewEdit_NamePlaceholder","z. B. Eisen & Blutbild","Например, Железо и кровь"),
("ViewEdit_NothingPicked","Noch nichts ausgewählt. Tippen Sie die Werte an, die in die Ansicht sollen.","Пока ничего не выбрано. Отметьте показатели, которые войдут в набор."),
("ViewEdit_OnePicked","1 Wert ausgewählt","Выбран 1 показатель"),
("ViewEdit_ManyPicked","{0} Werte ausgewählt · Reihenfolge wie ausgewählt","Выбрано показателей: {0} · порядок как при выборе"),
("ViewEdit_NoNameTitle","Name fehlt","Не хватает названия"),
("ViewEdit_NoNameBody","Geben Sie der Ansicht einen Namen.","Дайте набору название."),
("ViewEdit_EmptyTitle","Nichts ausgewählt","Ничего не выбрано"),
("ViewEdit_EmptyBody","Wählen Sie mindestens einen Wert aus.","Отметьте хотя бы один показатель."),
("ViewEdit_DeleteTitle","Ansicht löschen?","Удалить набор?"),
("ViewEdit_DeleteBody","Nur die Ansicht wird gelöscht. Die Messungen bleiben unberührt.","Удалится только набор. Сами измерения останутся нетронутыми."),
("Err_View","Ansicht konnte nicht geöffnet werden","Не удалось открыть набор"),
("Err_ViewSave","Ansicht konnte nicht gespeichert werden","Не удалось сохранить набор"),
("Err_ViewDelete","Ansicht konnte nicht gelöscht werden","Не удалось удалить набор"),
]
S += [
("Edit_MergeTitle","Für diesen Tag gibt es schon Werte","За это число уже есть значения"),
("Edit_MergeBody","Zum {0} sind diese Werte bereits erfasst – mit einem anderen Ergebnis:\n\n{1}\n\nSollen die neuen Werte die bisherigen ersetzen?","За {0} эти показатели уже записаны, и значения другие:\n\n{1}\n\nЗаменить прежние значения новыми?"),
("Edit_MergeReplace","Ersetzen","Заменить"),
("Edit_MergeKeep","Bisherige behalten","Оставить прежние"),
("Edit_MergeMore","… und {0} weitere","…и ещё {0}"),
]
S += [
("Edit_OtherUnits","Andere Einheit als bisher: {0}. Werte in verschiedenen Einheiten sind nicht vergleichbar – bitte prüfen.","Единицы отличаются от прежних: {0}. Значения в разных единицах несравнимы — проверьте."),
]
S += [
("Set_AboutTitle","Über die App","О приложении"),
("Set_AboutBody","Die Version entspricht dem Release auf GitHub. Alle Daten bleiben auf diesem Gerät.","Версия совпадает с выпуском на GitHub. Все данные остаются на этом устройстве."),
("Set_Version","Version {0} (Build {1})","Версия {0} (сборка {1})"),
]
S += [
("Trend_OnlyWithHistory","Nur mit Verlauf","Только с историей"),
("Matrix_NewView","＋ Neue Ansicht …","＋ Новый набор …"),
("Matrix_EditView","✎ Ausgewählte Ansicht ändern …","✎ Изменить выбранный набор …"),
("Set_HistoryBody","Alle erfassten Befunde, einzeln zum Öffnen und Ändern.","Все внесённые анализы — открыть и поправить любой."),
("Set_HistoryOpen","Befunde öffnen","Открыть анализы"),
]
S += [
("Imp_AmbiguousNumber","{0}: «{1}» wurde als Tausendertrennung gelesen. Bitte gegen den Befund prüfen.","{0}: «{1}» прочитано как разделитель тысяч. Сверьте с бланком."),
]
S += [
("Trend_MixedUnits","Achtung: die Werte stammen aus verschiedenen Einheiten und sind nicht vergleichbar. Die App rechnet nichts um.","Внимание: значения в разных единицах и несравнимы между собой. Приложение ничего не пересчитывает."),
("Set_BackupNever","Noch keine Sicherung erstellt.","Резервная копия ещё не делалась."),
("Set_BackupToday","Letzte Sicherung: heute.","Последняя резервная копия: сегодня."),
("Set_BackupYesterday","Letzte Sicherung: gestern.","Последняя резервная копия: вчера."),
("Set_BackupDaysAgo","Letzte Sicherung: vor {0} Tagen.","Последняя резервная копия: {0} дн. назад."),
("Set_BackupOverdue","Die Daten liegen nur auf diesem Gerät. Geht es verloren, ist der Verlauf weg.","Данные лежат только на этом устройстве. Потеряется телефон — история пропадёт."),
]
S += [
("Bp_Title","Blutdruck","Давление"),
("Bp_TitleNew","Blutdruck erfassen","Записать давление"),
("Bp_TitleExisting","Messung ändern","Изменить измерение"),
("Bp_Add","➕ Blutdruck erfassen","➕ Записать давление"),
("Bp_When","Datum und Uhrzeit","Дата и время"),
("Bp_Systolic","Systolisch (oben)","Верхнее"),
("Bp_Diastolic","Diastolisch (unten)","Нижнее"),
("Bp_Pulse","Puls","Пульс"),
("Bp_PulsePlaceholder","optional","необязательно"),
("Bp_PulseValue","{0} /min","{0} уд/мин"),
("Bp_Note","Notiz","Заметка"),
("Bp_NotePlaceholder","z. B. nach dem Aufstehen, linker Arm","Например: после сна, левая рука"),
("Bp_Empty","Noch keine Messung. Tragen Sie die erste ein – Uhrzeit wird mitgespeichert, damit sich morgens und abends unterscheiden lassen.","Пока ни одного измерения. Внесите первое — время сохраняется вместе с датой, чтобы утро и вечер не путались."),
("Bp_NoneYet","Noch nicht erfasst","Ещё не записано"),
("Bp_LastReading","{0} · {1}","{0} · {1}"),
("Bp_BadValuesTitle","Werte fehlen","Не хватает значений"),
("Bp_BadValuesBody","Tragen Sie den oberen und den unteren Wert als ganze Zahlen ein.","Впишите верхнее и нижнее значения целыми числами."),
("Bp_ImplausibleTitle","Bitte prüfen","Проверьте значения"),
("Bp_ImplausibleBody","Diese Zahlen ergeben keinen sinnvollen Blutdruck. Der untere Wert muss kleiner als der obere sein – vermutlich ein Zahlendreher.","Такие числа не складываются в осмысленное давление: нижнее должно быть меньше верхнего. Похоже на опечатку."),
("Bp_DeleteTitle","Messung löschen?","Удалить измерение?"),
("Bp_DeleteBody","Diese eine Messung wird gelöscht. Die übrigen bleiben.","Удалится это одно измерение. Остальные останутся."),
("Bp_TargetSummary","Hervorgehoben wird alles über {0}/{1}.","Подсвечивается всё выше {0}/{1}."),
("Bp_TargetBody","Ab welchem Wert eine Messung hervorgehoben wird. Tragen Sie hier ein, was Ihre Ärztin oder Ihr Arzt Ihnen genannt hat – die App stellt keine Diagnose und stuft nichts ein.","С какого значения измерение подсвечивать. Впишите то, что назвал ваш врач: приложение не ставит диагноз и не определяет степень."),
("Bp_TargetSave","Zielwert speichern","Сохранить порог"),
("Bp_TargetSavedTitle","Gespeichert","Сохранено"),
("Bp_TargetSavedBody","Hervorgehoben wird ab {0}/{1}.","Подсветка начинается с {0}/{1}."),
("Bp_BadTargetTitle","Zielwert prüfen","Проверьте порог"),
("Bp_BadTargetBody","Der obere Wert muss größer als der untere sein und beide im sinnvollen Bereich liegen.","Верхнее значение должно быть больше нижнего, и оба — в осмысленных пределах."),
("Bp_OpenDiary","Alle Messungen öffnen","Открыть все измерения"),
("Bp_Disclaimer","Die App speichert nur, was Sie eintragen. Sie bewertet den Blutdruck nicht und ersetzt keine ärztliche Beurteilung.","Приложение сохраняет только то, что вы внесли. Оно не оценивает давление и не заменяет заключение врача."),
("Err_Bp","Blutdruck konnte nicht geladen werden","Не удалось загрузить давление"),
("Err_BpSave","Messung konnte nicht gespeichert werden","Не удалось сохранить измерение"),
("Err_BpDelete","Messung konnte nicht gelöscht werden","Не удалось удалить измерение"),
]
S += [
("Csv_Time","Uhrzeit","Время"),
("Txt_Pressure","**Blutdruck** (Datum, Uhrzeit, oben, unten, Puls):","**Давление** (дата, время, верхнее, нижнее, пульс):"),
("Set_ExportPressure","Blutdruck als CSV","Давление в CSV"),
("Share_Pressure","Blutdruck-Tagebuch","Дневник давления"),
("Err_ExportPressure","Blutdruck konnte nicht exportiert werden","Не удалось выгрузить давление"),
]
S += [
("Bp_ChartNoData","Noch zu wenig Messungen für einen Verlauf","Пока мало измерений для графика"),
("Bp_ChartLegend","letzte {0} Messungen","последние {0} измерений"),
]
S += [
("Auto_Title","Automatische Sicherung","Автоматическая копия"),
("Auto_Body","Wählen Sie einmal einen Ordner. Beim Öffnen der App legt sie dort eine Sicherung ab – aber nur, wenn sich seit der letzten etwas geändert hat. Die App sendet nichts; wird der Ordner von einem Cloud-Dienst synchronisiert, erledigt das dieser Dienst.","Выберите папку один раз. При открытии приложения туда кладётся копия — но только если с прошлого раза что-то изменилось. Приложение ничего не отправляет; если папку синхронизирует облако, это делает оно, а не мы."),
("Auto_NoFolder","Kein Ordner gewählt — es wird nichts automatisch gesichert.","Папка не выбрана — автоматически ничего не сохраняется."),
("Auto_Summary","Ordner: {0}. Letzte automatische Sicherung: {1}.","Папка: {0}. Последняя автоматическая копия: {1}."),
("Auto_NeverYet","noch keine","ещё не было"),
("Auto_ChooseFolder","Ordner wählen","Выбрать папку"),
("Auto_Now","Jetzt sichern","Сохранить сейчас"),
("Auto_Forget","Ordner vergessen","Забыть папку"),
("Auto_DoneTitle","Gesichert","Сохранено"),
("Auto_DoneBody","Die Sicherung liegt in «{0}».","Копия лежит в «{0}»."),
("Auto_Failed","Die Sicherung konnte nicht abgelegt werden. Vermutlich gibt es den Ordner nicht mehr oder die Berechtigung wurde entzogen — wählen Sie ihn erneut.","Не удалось положить копию. Скорее всего папки больше нет или разрешение отозвано — выберите её заново."),
("Auto_ForgetTitle","Ordner vergessen?","Забыть папку?"),
("Auto_ForgetBody","Automatische Sicherungen hören auf. Bereits abgelegte Dateien bleiben, wo sie sind.","Автоматические копии прекратятся. Уже созданные файлы останутся на месте."),
]
S += [
("Edit_AmbiguousTitle","Punkt als Tausendertrennung gelesen","Точка прочитана как разделитель тысяч"),
("Edit_AmbiguousBody","Diese Eingaben sind so verstanden worden:\n\n{0}\n\nIst das gemeint? Für einen Dezimalwert schreiben Sie ein Komma.","Эти значения поняты так:\n\n{0}\n\nВы это имели в виду? Для дробного числа поставьте запятую."),
("Edit_AmbiguousAccept","So übernehmen","Так и оставить"),
("Edit_AmbiguousBack","Zurück zur Eingabe","Вернуться к вводу"),
]

# Запрос для чат-бота. Он тоже попадает человеку в руки — значит, тоже переводится.
# Разбор ответа понимает и немецкие, и русские слова шапки, поэтому перевод безопасен.
PROMPT_DE = """Unten ist das Foto eines Laborbefunds. Übertrage die Daten daraus als reinen Text
genau in diesem Format und ergänze nichts von dir aus.

Die ersten beiden Zeilen:
Datum: TT.MM.JJJJ
Labor: Name oder Strich

Danach eine Zeile je Wert, die Felder mit einem senkrechten Strich getrennt:
Bezeichnung | Ergebnis | Einheit | Referenz von | Referenz bis

Regeln:
- Das Ergebnis ist nur eine Zahl. Ist es nicht numerisch, schreibe es als Wort
  («negativ», «Spuren»).
- Der Referenzbereich sind zwei getrennte Zahlen. Ist er einseitig («bis 5,2»),
  fülle nur das passende Feld, das andere bleibt leer.
- Rate nichts und rechne nichts um: Ist ein Feld im Befund nicht lesbar, lass es leer.
- Keine Tabellenüberschrift, keine Nummerierung, keine Erläuterungen, keine
  Schlussfolgerungen und keine Empfehlungen. Nur die Datenzeilen."""

PROMPT_RU = """Ниже — фотография бланка анализов. Перепиши данные из неё обычным текстом
ровно в этом формате и ничего не добавляй от себя.

Первые две строки:
Дата: ДД.ММ.ГГГГ
Лаборатория: название или прочерк

Дальше по строке на каждый показатель, поля разделены вертикальной чертой:
Название | Результат | Единица | Норма от | Норма до

Правила:
- Результат — только число. Если он не числовой, напиши его словом
  («отрицательно», «следы»).
- Норма — два отдельных числа. Если она односторонняя («до 5,2»),
  заполни только подходящее поле, второе оставь пустым.
- Ничего не угадывай и не пересчитывай: не читается поле в бланке — оставь его пустым.
- Без заголовка таблицы, без нумерации, без пояснений, без выводов
  и без рекомендаций. Только строки с данными."""

FREE_DE = """- Übernimm die Bezeichnungen genau wie im Befund: nicht übersetzen, nicht abkürzen,
  nicht umbenennen."""

FREE_RU = """- Названия переписывай точно как в бланке: не переводи, не сокращай,
  не переименовывай."""

KNOWN_DE = """- Gleicht ein Wert einer Bezeichnung aus der Liste unten – auch abgekürzt («Hb»),
  in einer anderen Sprache («Hemoglobin») oder anders geschrieben –, dann schreibe
  die Bezeichnung genau so, wie sie in der Liste steht. Nur so landet der Wert in
  derselben Zeile meiner Tabelle wie bisher und nicht in einer zweiten daneben.
- Steht ein Wert nicht in der Liste, übernimm seine Bezeichnung unverändert
  aus dem Befund: nicht übersetzen, nicht abkürzen, nicht umbenennen.
- Zahl und Einheit bleiben immer so, wie sie im Befund gedruckt sind. Rechne
  auch dann nicht um, wenn die Liste eine andere Einheit nennt – schreibe die
  Einheit des Befunds hin. Die Angleichung betrifft ausschließlich die Bezeichnung.

Bezeichnungen, die ich schon führe (Bezeichnung | Einheit):"""

KNOWN_RU = """- Если показатель совпадает с названием из списка ниже — пусть даже сокращённым
  («Hb»), на другом языке («Hemoglobin») или написанным иначе, — пиши название
  ровно так, как оно стоит в списке. Только тогда значение попадёт в ту же строку
  моей таблицы, что и раньше, а не во вторую рядом.
- Если показателя в списке нет, перепиши его название из бланка без изменений:
  не переводи, не сокращай, не переименовывай.
- Число и единица всегда остаются такими, как напечатаны в бланке. Не пересчитывай,
  даже если в списке стоит другая единица, — пиши единицу бланка. Приведение
  касается только названия.

Названия, которые у меня уже есть (Название | Единица):"""

S += [
("Imp_Prompt", PROMPT_DE, PROMPT_RU),
("Imp_PromptFreeNames", FREE_DE, FREE_RU),
("Imp_PromptKnownNames", KNOWN_DE, KNOWN_RU),
]
# Примечания встроенного справочника: в базе лежат эти ключи, перевод подставляется при выводе.
S += [
("Seed_Note_Rbc","Bei Frauen liegt die Untergrenze niedriger","У женщин нижняя граница ниже"),
("Seed_Note_Hgb","Frauen 12,0–16,0 · Männer 13,5–17,5","Женщины 12,0–16,0 · мужчины 13,5–17,5"),
("Seed_Note_Ldl","Zielwert hängt vom kardiovaskulären Risiko ab","Целевое значение зависит от сердечно-сосудистого риска"),
("Seed_Note_Hdl","Bei Frauen ab 50","У женщин — от 50"),
("Seed_Note_Glu","Nüchtern","Натощак"),
("Seed_Note_Tsto","Bereich für Männer","Диапазон для мужчин"),
("Seed_Note_Cort","Morgendliche Abnahme","Утренний забор"),
]
# Подписи для чтения с экрана: кнопки со значком и графики иначе остаются немыми.
S += [
("A11y_RemoveRow","Zeile entfernen","Убрать строку"),
("A11y_TrendChart","Verlauf des Werts als Diagramm","График показателя"),
("A11y_PressureChart","Verlauf des Blutdrucks als Diagramm","График давления"),
]

# ИИ-блок живёт на экране динамики: отправляется то, что осталось после фильтров.
S += [
("Ai_Title","KI fragen","Спросить у ИИ"),
("Ai_Body","Gesendet wird genau das, was die Filter übrig lassen. Die App stellt daraus eine Tabelle als Text zusammen und öffnet die Teilen-Auswahl – wohin der Text geht, entscheiden Sie.","Отправляется ровно то, что осталось после фильтров. Приложение соберёт из этого таблицу текстом и откроет системный список приложений — куда отправить, решаете вы."),
("Ai_Button","🤖 Auswahl an KI-Chat senden","🤖 Отправить выбранное в ИИ-чат"),
("Ai_Off","KI-Assistent ist aus. Die Daten verlassen das Gerät nicht.","ИИ-помощник выключен. Данные не покидают устройство."),
("Ai_On","KI-Assistent: {0}. Die Berechtigung lässt sich in den Einstellungen widerrufen.","ИИ-помощник: {0}. Разрешение можно отозвать в настройках."),
("Ai_NothingSelected","Die Filter lassen nichts übrig – es gibt nichts zu senden.","После фильтров ничего не осталось — отправлять нечего."),
]
# Фильтры и жесты на экране динамики.
S += [
("Trend_Search","Wert suchen","Найти показатель"),
("Trend_FilterOut","Außerhalb","Вне нормы"),
("Trend_FilterHigh","Zu hoch","Выше нормы"),
("Trend_FilterLow","Zu niedrig","Ниже нормы"),
("Trend_NothingFound","Kein Wert passt zu den Filtern.","Под фильтры ничего не подошло."),
("Trend_HiddenCount","{0} ausgeblendet · zurückholen im Werteverzeichnis","Скрыто: {0} · вернуть можно в справочнике"),
("Swipe_Favorite","★ Merken","★ В избранное"),
("Swipe_Unfavorite","☆ Nicht mehr merken","☆ Убрать из избранного"),
("Swipe_Hide","Ausblenden","Скрыть"),
]
# Сводка на главном экране вместо длинного списка.
S += [
("Dash_HighCount","{0} über dem Referenzbereich","{0} выше нормы"),
("Dash_LowCount","{0} unter dem Referenzbereich","{0} ниже нормы"),
("Dash_NoneOut","Alle Werte liegen im Referenzbereich Ihres Befunds.","Все показатели — в пределах норм из вашего бланка."),
("Dash_OpenTrends","Tippen für alle Werte","Нажмите, чтобы увидеть все показатели"),
]
S += [
("Txt_Selection","Unten stehen nur ausgewählte Werte, nicht der vollständige Befund.","Ниже — только выбранные показатели, а не весь бланк целиком."),
]
