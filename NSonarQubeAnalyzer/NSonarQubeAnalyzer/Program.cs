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
            var rules = (from e in xmlIn.Descendants("Rule")
                         select e.Elements("Key").Single().Value)
                         .ToImmutableHashSet();

            var diagnosticAnalyzersBuilder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
            if (rules.Contains("SwitchWithoutDefault"))
            {
                diagnosticAnalyzersBuilder.Add(new SwitchWithoutDefault());
            }
            if (rules.Contains("S1301"))
            {
                diagnosticAnalyzersBuilder.Add(new AtLeastThreeCasesInSwitch());
            }
            if (rules.Contains("BreakOutsideSwitch"))
            {
                diagnosticAnalyzersBuilder.Add(new BreakOutsideSwitch());
            }
            if (rules.Contains("S1125"))
            {
                diagnosticAnalyzersBuilder.Add(new UnnecessaryBooleanLiteral());
            }
            if (rules.Contains("S1145"))
            {
                diagnosticAnalyzersBuilder.Add(new IfConditionalAlwaysTrueOrFalse());
            }
            if (rules.Contains("AsyncAwaitIdentifier"))
            {
                diagnosticAnalyzersBuilder.Add(new AsyncAwaitIdentifier());
            }
            if (rules.Contains("AssignmentInsideSubExpression"))
            {
                diagnosticAnalyzersBuilder.Add(new AssignmentInsideSubExpression());
            }
            if (rules.Contains("S126"))
            {
                diagnosticAnalyzersBuilder.Add(new ElseIfWithoutElse());
            }
            if (rules.Contains("S1116"))
            {
                diagnosticAnalyzersBuilder.Add(new EmptyStatement());
            }
            if (rules.Contains("S121"))
            {
                diagnosticAnalyzersBuilder.Add(new UseCurlyBraces());
            }
            if (rules.Contains("S1109"))
            {
                diagnosticAnalyzersBuilder.Add(new RightCurlyBraceStartsLine());
            }
            if (rules.Contains("TabCharacter"))
            {
                diagnosticAnalyzersBuilder.Add(new TabCharacter());
            }
            if (rules.Contains("S1186"))
            {
                diagnosticAnalyzersBuilder.Add(new EmptyMethod());
            }
            if (rules.Contains("S127"))
            {
                diagnosticAnalyzersBuilder.Add(new ForLoopCounterChanged());
            }
            if (rules.Contains("FileLoc"))
            {
                var parameters = from e in xmlIn.Descendants("Rule")
                                 where "FileLoc".Equals(e.Elements("Key").Single().Value)
                                 select e.Descendants("Parameter");
                var maximum = (from e in parameters
                               where "maximumFileLocThreshold".Equals(e.Elements("Key").Single().Value)
                               select e.Elements("Value").Single().Value)
                              .Single();

                var diagnostic = new FileLines();
                diagnostic.Maximum = int.Parse(maximum);
                diagnosticAnalyzersBuilder.Add(diagnostic);
            }
            if (rules.Contains("LineLength"))
            {
                var parameters = from e in xmlIn.Descendants("Rule")
                                 where "LineLength".Equals(e.Elements("Key").Single().Value)
                                 select e.Descendants("Parameter");
                var maximum = (from e in parameters
                               where "maximumLineLength".Equals(e.Elements("Key").Single().Value)
                               select e.Elements("Value").Single().Value)
                              .Single();

                var diagnostic = new LineLength();
                diagnostic.Maximum = int.Parse(maximum);
                diagnosticAnalyzersBuilder.Add(diagnostic);
            }
            if (rules.Contains("S1479"))
            {
                var parameters = from e in xmlIn.Descendants("Rule")
                                 where "S1479".Equals(e.Elements("Key").Single().Value)
                                 select e.Descendants("Parameter");
                var maximum = (from e in parameters
                               where "maximum".Equals(e.Elements("Key").Single().Value)
                               select e.Elements("Value").Single().Value)
                              .Single();

                var diagnostic = new TooManyLabelsInSwitch();
                diagnostic.Maximum = int.Parse(maximum);
                diagnosticAnalyzersBuilder.Add(diagnostic);
            }
            if (rules.Contains("S107"))
            {
                var parameters = from e in xmlIn.Descendants("Rule")
                                 where "S107".Equals(e.Elements("Key").Single().Value)
                                 select e.Descendants("Parameter");
                var maximum = (from e in parameters
                               where "max".Equals(e.Elements("Key").Single().Value)
                               select e.Elements("Value").Single().Value)
                              .Single();

                var diagnostic = new TooManyParameters();
                diagnostic.Maximum = int.Parse(maximum);
                diagnosticAnalyzersBuilder.Add(diagnostic);
            }
            if (rules.Contains("ClassName2"))
            {
                var parameters = from e in xmlIn.Descendants("Rule")
                                 where "ClassName2".Equals(e.Elements("Key").Single().Value)
                                 select e.Descendants("Parameter");
                var convention = (from e in parameters
                                  where "format".Equals(e.Elements("Key").Single().Value)
                                  select e.Elements("Value").Single().Value)
                              .Single();

                var diagnostic = new ClassName();
                diagnostic.Convention = convention;
                diagnosticAnalyzersBuilder.Add(diagnostic);
            }
            if (rules.Contains("MethodName2"))
            {
                var parameters = from e in xmlIn.Descendants("Rule")
                                 where "MethodName2".Equals(e.Elements("Key").Single().Value)
                                 select e.Descendants("Parameter");
                var convention = (from e in parameters
                                  where "format".Equals(e.Elements("Key").Single().Value)
                                  select e.Elements("Value").Single().Value)
                              .Single();

                var diagnostic = new MethodName();
                diagnostic.Convention = convention;
                diagnosticAnalyzersBuilder.Add(diagnostic);
            }
            if (rules.Contains("MagicNumber"))
            {
                var parameters = from e in xmlIn.Descendants("Rule")
                                 where "MagicNumber".Equals(e.Elements("Key").Single().Value)
                                 select e.Descendants("Parameter");
                var exceptions = (from e in parameters
                                  where "exceptions".Equals(e.Elements("Key").Single().Value)
                                  select e.Elements("Value").Single().Value)
                              .Single();

                var diagnostic = new MagicNumber();
                diagnostic.Exceptions = exceptions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToImmutableHashSet();
                diagnosticAnalyzersBuilder.Add(diagnostic);
            }
            if (rules.Contains("S1067"))
            {
                var parameters = from e in xmlIn.Descendants("Rule")
                                 where "S1067".Equals(e.Elements("Key").Single().Value)
                                 select e.Descendants("Parameter");
                var maximum = (from e in parameters
                               where "max".Equals(e.Elements("Key").Single().Value)
                               select e.Elements("Value").Single().Value)
                              .Single();

                var diagnostic = new ExpressionComplexity();
                diagnostic.Maximum = int.Parse(maximum);
                diagnosticAnalyzersBuilder.Add(diagnostic);
            }
            if (rules.Contains("S108"))
            {
                diagnosticAnalyzersBuilder.Add(new EmptyNestedBlock());
            }
            if (rules.Contains("ParameterAssignedTo"))
            {
                diagnosticAnalyzersBuilder.Add(new ParameterAssignedTo());
            }
            if (rules.Contains("S1481"))
            {
                diagnosticAnalyzersBuilder.Add(new UnusedLocalVariable());
            }
            if (rules.Contains("CommentedCode"))
            {
                diagnosticAnalyzersBuilder.Add(new CommentedOutCode());
            }

            var commentRegexpRules = from e in xmlIn.Descendants("Rule")
                                     where "CommentRegularExpression2".Equals((e.Elements("ParentKey").SingleOrDefault() ?? XElement.Parse("<Dummy />")).Value)
                                     select e;
            if (commentRegexpRules.Any())
            {
                var builder = ImmutableArray.CreateBuilder<CommentRegularExpressionRule>();
                foreach (var commentRegexpRule in commentRegexpRules)
                {
                    string key = commentRegexpRule.Elements("Key").Single().Value;
                    var parameters = commentRegexpRule.Descendants("Parameter");
                    string message = (from e in parameters
                                      where "message".Equals(e.Elements("Key").Single().Value)
                                      select e.Elements("Value").Single().Value)
                                  .Single();
                    string regularExpression = (from e in parameters
                                                where "regularExpression".Equals(e.Elements("Key").Single().Value)
                                                select e.Elements("Value").Single().Value)
                                  .Single();

                    builder.Add(
                        new CommentRegularExpressionRule
                        {
                            // TODO: Add rule description
                            Descriptor = new DiagnosticDescriptor(key, "TODO", message, "SonarQube", DiagnosticSeverity.Warning, true),
                            RegularExpression = regularExpression
                        });

                    var diagnostic = new CommentRegularExpression();
                    diagnostic.Rules = builder.ToImmutable();
                    diagnosticAnalyzersBuilder.Add(diagnostic);
                }
            }
            if (rules.Contains("FunctionComplexity"))
            {
                var parameters = from e in xmlIn.Descendants("Rule")
                                 where "FunctionComplexity".Equals(e.Elements("Key").Single().Value)
                                 select e.Descendants("Parameter");
                var maximum = (from e in parameters
                               where "maximumFunctionComplexityThreshold".Equals(e.Elements("Key").Single().Value)
                               select e.Elements("Value").Single().Value)
                              .Single();

                var diagnostic = new FunctionComplexity();
                diagnostic.Maximum = int.Parse(maximum);
                diagnosticAnalyzersBuilder.Add(diagnostic);
            }

            var diagnosticsRunner = new DiagnosticsRunner(diagnosticAnalyzersBuilder.ToImmutableArray());

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

                    xmlOut.WriteStartElement("CopyPasteTokens");
                    foreach (var token in metrics.CopyPasteTokens())
                    {
                        xmlOut.WriteStartElement("Token");
                        xmlOut.WriteElementString("Value", token.Item1);
                        xmlOut.WriteElementString("Line", token.Item2.ToString());
                        xmlOut.WriteEndElement();
                    }
                    xmlOut.WriteEndElement();

                    xmlOut.WriteEndElement();
                }
                xmlOut.WriteEndElement();

                xmlOut.WriteEndElement();
                xmlOut.WriteEndDocument();

                xmlOut.Flush();
            }
        }
    }
}
