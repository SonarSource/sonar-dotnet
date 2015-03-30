using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace NSonarQubeAnalyzer
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var configuration = new Configuration(XDocument.Load(args[0]));

            var xmlOutSettings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8, 
                Indent = true, 
                IndentChars = "  "
            };

            using (var xmlOut = XmlWriter.Create(args[1], xmlOutSettings))
            {
                xmlOut.WriteComment("This XML format is not an API");
                xmlOut.WriteStartElement("AnalysisOutput");

                xmlOut.WriteStartElement("Files");
                var n = 0;
                foreach (var file in configuration.Files)
                {
                    xmlOut.Flush();
                    Console.WriteLine(n + "/" + configuration.Files.Count() + " files analyzed, starting to analyze: " + file);
                    n++;

                    try
                    {
                        var solution = CompilationHelper.GetSolutionFromFiles(file);

                        var compilation = solution.Projects.First().GetCompilationAsync().Result;
                        var syntaxTree = compilation.SyntaxTrees.First();
                        var diagnosticsRunner = new DiagnosticsRunner(configuration.Analyzers(solution));

                        var metrics = new Metrics(syntaxTree);

                        xmlOut.WriteStartElement("File");
                        xmlOut.WriteElementString("Path", file);

                        xmlOut.WriteStartElement("Metrics");

                        xmlOut.WriteElementString("Lines", metrics.Lines().ToString());
                        xmlOut.WriteElementString("Classes", metrics.Classes().ToString());
                        xmlOut.WriteElementString("Accessors", metrics.Accessors().ToString());
                        xmlOut.WriteElementString("Statements", metrics.Statements().ToString());
                        xmlOut.WriteElementString("Functions", metrics.Functions().ToString());
                        xmlOut.WriteElementString("PublicApi", metrics.PublicApi().ToString());
                        xmlOut.WriteElementString("PublicUndocumentedApi", metrics.PublicUndocumentedApi().ToString());

                        var complexity = metrics.Complexity();
                        xmlOut.WriteElementString("Complexity", complexity.ToString());

                        // TODO This is a bit ridiculous, but is how SonarQube works
                        var fileComplexityDistribution = new Distribution(0, 5, 10, 20, 30, 60, 90);
                        fileComplexityDistribution.Add(complexity);
                        xmlOut.WriteElementString("FileComplexityDistribution", fileComplexityDistribution.ToString());

                        xmlOut.WriteElementString("FunctionComplexityDistribution", metrics.FunctionComplexityDistribution().ToString());

                        var comments = metrics.Comments(configuration.IgnoreHeaderComments);
                        xmlOut.WriteStartElement("Comments");
                        xmlOut.WriteStartElement("NoSonar");
                        foreach (var line in comments.NoSonar)
                        {
                            xmlOut.WriteElementString("Line", line.ToString());
                        }
                        xmlOut.WriteEndElement();
                        xmlOut.WriteStartElement("NonBlank");
                        foreach (var line in comments.NonBlank)
                        {
                            xmlOut.WriteElementString("Line", line.ToString());
                        }
                        xmlOut.WriteEndElement();
                        xmlOut.WriteEndElement();

                        xmlOut.WriteStartElement("LinesOfCode");
                        foreach (var line in metrics.LinesOfCode())
                        {
                            xmlOut.WriteElementString("Line", line.ToString());
                        }
                        xmlOut.WriteEndElement();

                        xmlOut.WriteEndElement();

                        xmlOut.WriteStartElement("Issues");
                        
                        foreach (var diagnostic in diagnosticsRunner.GetDiagnostics(compilation))
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
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("Failed to analyze the file: " + file);
                        Console.Error.WriteLine(e);
                        return 1;
                    }
                }

                xmlOut.WriteEndElement();

                xmlOut.WriteEndElement();
                xmlOut.WriteEndDocument();

                xmlOut.Flush();
                return 0;
            }
        }
    }
}
