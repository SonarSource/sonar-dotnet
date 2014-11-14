using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Threading;

namespace NSonarQubeAnalyzer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var xmlIn = XDocument.Load(args[0]);
            var settings = (from e in xmlIn.Descendants("Setting")
                            select new { Key = e.Element("Key").Value, Value = e.Element("Value").Value })
                           .ToImmutableDictionary(e => e.Key, e => e.Value);
            var files = from e in xmlIn.Descendants("File")
                        select e.Value;
            bool ignoreHeaderComments = "true".Equals(settings["sonar.cs.ignoreHeaderComments"]);

            var xmlOutSettings = new XmlWriterSettings();
            xmlOutSettings.Encoding = Encoding.UTF8;
            xmlOutSettings.Indent = true;
            xmlOutSettings.IndentChars = "  ";
            using (var xmlOut = XmlWriter.Create(args[1], xmlOutSettings))
            {
                xmlOut.WriteComment("This XML format is not an API");
                xmlOut.WriteStartElement("AnalysisOutput");

                xmlOut.WriteStartElement("Files");
                foreach (var file in files)
                {
                    xmlOut.WriteStartElement("File");
                    xmlOut.WriteElementString("Path", file);

                    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(file, Encoding.UTF8));
                    Metrics metrics = new Metrics(syntaxTree);
                    xmlOut.WriteStartElement("Metrics");

                    xmlOut.WriteElementString("Lines", metrics.Lines().ToString());

                    FileComments comments = metrics.Comments(ignoreHeaderComments);
                    xmlOut.WriteStartElement("Comments");
                    xmlOut.WriteStartElement("NoSonar");
                    foreach (int line in comments.NoSonar)
                    {
                        xmlOut.WriteElementString("Line", line.ToString());
                    }
                    xmlOut.WriteEndElement();
                    xmlOut.WriteStartElement("NonBlank");
                    foreach (int line in comments.NonBlank)
                    {
                        xmlOut.WriteElementString("Line", line.ToString());
                    }
                    xmlOut.WriteEndElement();
                    xmlOut.WriteEndElement();

                    xmlOut.WriteEndElement();

                    xmlOut.WriteEndElement();
                }
                xmlOut.WriteEndElement();

                xmlOut.WriteEndElement();
                xmlOut.WriteEndDocument();
            }
        }
    }
}
