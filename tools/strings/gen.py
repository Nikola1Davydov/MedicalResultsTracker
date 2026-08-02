# -*- coding: utf-8 -*-
"""
Собирает AppResources.resx, AppResources.ru.resx и S.cs из таблицы strings.py.

Запуск из корня репозитория:  python3 tools/strings/gen.py

Ключ, которого нет в таблице, не соберётся; строка без перевода не добавится
физически — в таблице это один кортеж на оба языка.
"""
import io, os, sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from strings import S

ROOT = os.path.join(
    os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))),
    "MedicalResultsTracker", "Resources", "Strings")

HEADER = '''<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="metadata">
            <xsd:complexType>
              <xsd:sequence><xsd:element name="value" type="xsd:string" minOccurs="0" /></xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="assembly">
            <xsd:complexType>
              <xsd:attribute name="alias" type="xsd:string" />
              <xsd:attribute name="name" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence><xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" /></xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
'''


def esc(v):
    return v.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def write_resx(path, idx):
    out = io.StringIO()
    out.write(HEADER)
    for row in S:
        out.write('  <data name="%s" xml:space="preserve">\n    <value>%s</value>\n  </data>\n'
                  % (row[0], esc(row[idx])))
    out.write("</root>\n")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(out.getvalue())


def write_cs(path):
    out = io.StringIO()
    out.write('''using System.Globalization;
using System.Resources;

namespace MedicalResultsTracker.Resources.Strings
{
    /// <summary>
    /// Тексты интерфейса. Немецкий — язык по умолчанию, русский подхватывается,
    /// если он выбран в настройках или стоит в системе.
    ///
    /// Файл собран из tools/strings/strings.py вместе с обоими .resx — править нужно
    /// таблицу и заново запускать tools/strings/gen.py. Правка здесь или в resx
    /// пропадёт при следующей сборке строк.
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
''')
    for key, de, ru in S:
        doc = esc(" ".join(de.split())[:70])
        out.write('\n        /// <summary>%s</summary>\n        public static string %s => Get("%s");\n' % (doc, key, key))
    out.write("    }\n}\n")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(out.getvalue())


keys = [r[0] for r in S]
dupes = {k for k in keys if keys.count(k) > 1}
if dupes:
    print("ДУБЛИ КЛЮЧЕЙ:", dupes)
    sys.exit(1)

write_resx(os.path.join(ROOT, "AppResources.resx"), 1)
write_resx(os.path.join(ROOT, "AppResources.ru.resx"), 2)
write_cs(os.path.join(ROOT, "S.cs"))
print("ключей:", len(S))
