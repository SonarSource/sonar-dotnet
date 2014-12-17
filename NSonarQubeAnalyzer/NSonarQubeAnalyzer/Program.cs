namespace NSonarQubeAnalyzer
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

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

            ImmutableArray<DiagnosticAnalyzer> activatedRules = ConfigureActivatedRules(xmlIn);

            var diagnosticsRunner = new DiagnosticsRunner(activatedRules);

            var xmlOutSettings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true, IndentChars = "  " };

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
                    xmlOut.WriteElementString("Classes", metrics.Classes().ToString());
                    xmlOut.WriteElementString("Accessors", metrics.Accessors().ToString());
                    xmlOut.WriteElementString("Statements", metrics.Statements().ToString());
                    xmlOut.WriteElementString("Functions", metrics.Functions().ToString());
                    xmlOut.WriteElementString("PublicApi", metrics.PublicApi().ToString());
                    xmlOut.WriteElementString("PublicUndocumentedApi", metrics.PublicUndocumentedApi().ToString());
                    xmlOut.WriteElementString("Complexity", metrics.Complexity().ToString());

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

                    xmlOut.WriteStartElement("LinesOfCode");
                    foreach (int line in metrics.LinesOfCode())
                    {
                        xmlOut.WriteElementString("Line", line.ToString());
                    }
                    xmlOut.WriteEndElement();

                    xmlOut.WriteEndElement();

                    xmlOut.WriteStartElement("Issues");
                    foreach (var diagnostic in diagnosticsRunner.GetDiagnostics(syntaxTree))
                    {
                        xmlOut.WriteStartElement("Issue");
                        xmlOut.WriteElementString("Id", diagnostic.Id);
                        if (diagnostic.Location != Location.None)
                        {
                            xmlOut.WriteElementString("Line", (diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1).ToString());
                        }
                        xmlOut.WriteElementString("Message", diagnostic.GetMessage());
                        xmlOut.WriteEndElement();
                    }
                    xmlOut.WriteEndElement();

                    xmlOut.WriteEndElement();
                }
                xmlOut.WriteEndElement();

                xmlOut.WriteEndElement();
                xmlOut.WriteEndDocument();
            }
        }

        /// <summary>
        /// Create and configure instances of all rules that are activated
        /// </summary>
        /// <param name="xmlIn">XML settings</param>
        /// <returns>All activated rules</returns>
        private static ImmutableArray<DiagnosticAnalyzer> ConfigureActivatedRules(XDocument xmlIn)
        {
            var rules = (from e in xmlIn.Descendants("Rule")
                         select e.Elements("Key").Single().Value)
                         .ToImmutableHashSet();

            var diagnosticAnalyzersBuilder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();

            var superType = typeof(DiagnosticsRule);
            var activatedRules = from type in Assembly.GetExecutingAssembly().ExportedTypes
                                 where superType.IsAssignableFrom(type) && !type.IsAbstract
                                 let rule = (DiagnosticsRule)Activator.CreateInstance(type)
                                 where rules.Contains(rule.RuleId)
                                 select rule;

            foreach (DiagnosticsRule rule in activatedRules)
            {
                rule.Configure(xmlIn);
                diagnosticAnalyzersBuilder.Add(rule);
            }

            return diagnosticAnalyzersBuilder.ToImmutable();
        }
    }
}