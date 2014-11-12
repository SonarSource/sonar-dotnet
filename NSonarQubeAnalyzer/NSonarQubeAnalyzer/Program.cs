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
                    var rules = ImmutableArray.Create<IDiagnosticAnalyzer>(new ClassName());
                    var cancellationToken = new CancellationTokenSource().Token;
                    // TODO Add using back
                    var options = new AnalyzerOptions(ImmutableArray.Create<AdditionalStream>(), ImmutableDictionary.Create<string, string>());
                    var driver = new AnalyzerDriver<SyntaxKind>(rules, n => n.CSharpKind(), options, cancellationToken, null);
                    {
                        Compilation compilation = CSharpCompilation.Create(null, ImmutableArray.Create(syntaxTree));
                        //compilation = compilation.AddReferences(new MetadataReference[] { new MetadataFileReference(typeof(object).Assembly.Location) });
                        compilation = compilation.WithEventQueue(driver.CompilationEventQueue);

                        var result = compilation.GetDiagnostics(cancellationToken);
                        Console.WriteLine("Diagnostics from compilation: " + result.Length);
                        foreach (var diagnostic in result)
                        {
                            Console.WriteLine("  - " + diagnostic);
                        }

                        result = driver.GetDiagnosticsAsync().Result;
                        Console.WriteLine("Diagnostics from analysis: " + result.Length);
                        foreach (var diagnostic in result)
                        {
                            Console.WriteLine("  - " + diagnostic);
                        }
                    }

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
