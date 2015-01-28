using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Globalization;

namespace NSonarQubeAnalyzer
{
    public class Configuration
    {
        private ImmutableArray<DiagnosticAnalyzer> ParameterLessAnalyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
            new SwitchWithoutDefault(),
            new AtLeastThreeCasesInSwitch(),
            new BreakOutsideSwitch(),
            new UnnecessaryBooleanLiteral(),
            new IfConditionalAlwaysTrueOrFalse(),
            new AsyncAwaitIdentifier(),
            new AssignmentInsideSubExpression(),
            new ElseIfWithoutElse(),
            new EmptyStatement(),
            new UseCurlyBraces(),
            new RightCurlyBraceStartsLine(),
            new TabCharacter(),
            new EmptyMethod(),
            new ForLoopCounterChanged(),
            new EmptyNestedBlock(),
            new ParameterAssignedTo(),
            new UnusedLocalVariable(),
            new CommentedOutCode()
        );

        public bool IgnoreHeaderComments { private set; get; }
        public IImmutableList<string> Files { private set; get; }
        public IImmutableSet<string> AnalyzerIds { private set; get; }
        public IImmutableDictionary<string, List<IImmutableDictionary<string, string>>> Parameters { private set; get; }

        public Configuration(XDocument xml)
        {
            var settings = (from e in xml.Descendants("Setting")
                            select new { Key = e.Element("Key").Value, Value = e.Element("Value").Value })
                           .ToImmutableDictionary(e => e.Key, e => e.Value);

            IgnoreHeaderComments = "true".Equals(settings["sonar.cs.ignoreHeaderComments"]);

            Files = xml.Descendants("File").Select(e => e.Value).ToImmutableList();

            AnalyzerIds = xml.Descendants("Rule").Select(e => e.Elements("Key").Single().Value).ToImmutableHashSet();

            var builder = ImmutableDictionary.CreateBuilder<string, List<IImmutableDictionary<string, string>>>();
            foreach (var rule in xml.Descendants("Rule").Where(e => e.Elements("Parameters").Any()))
            {
                var analyzerId = rule.Elements("Key").Single().Value;

                var parameters = rule
                                 .Elements("Parameters").Single()
                                 .Elements("Parameter")
                                 .ToImmutableDictionary(e => e.Elements("Key").Single().Value, e => e.Elements("Value").Single().Value);

                if (!builder.ContainsKey(analyzerId))
                {
                    builder.Add(analyzerId, new List<IImmutableDictionary<string, string>>());
                }
                builder[analyzerId].Add(parameters);
            }
            Parameters = builder.ToImmutable();
        }

        public ImmutableArray<DiagnosticAnalyzer> Analyzers()
        {
            var builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();

            foreach (var parameterLessAnalyzer in ParameterLessAnalyzers)
            {
                if (AnalyzerIds.Contains(parameterLessAnalyzer.SupportedDiagnostics.Single().Id))
                {
                    builder.Add(parameterLessAnalyzer);
                }
            }

            {
                var analyzer = new FileLines();
                if (AnalyzerIds.Contains(analyzer.SupportedDiagnostics.Single().Id))
                {
                    analyzer.Maximum = int.Parse(
                        Parameters[analyzer.SupportedDiagnostics.Single().Id].Single()["maximumFileLocThreshold"],
                        NumberStyles.None, CultureInfo.InvariantCulture);
                    builder.Add(analyzer);
                }
            }

            {
                var analyzer = new LineLength();
                if (AnalyzerIds.Contains(analyzer.SupportedDiagnostics.Single().Id))
                {
                    analyzer.Maximum = int.Parse(
                        Parameters[analyzer.SupportedDiagnostics.Single().Id].Single()["maximumLineLength"],
                        NumberStyles.None, CultureInfo.InvariantCulture);
                    builder.Add(analyzer);
                }
            }

            {
                var analyzer = new TooManyLabelsInSwitch();
                if (AnalyzerIds.Contains(analyzer.SupportedDiagnostics.Single().Id))
                {
                    analyzer.Maximum = int.Parse(
                        Parameters[analyzer.SupportedDiagnostics.Single().Id].Single()["maximum"],
                        NumberStyles.None, CultureInfo.InvariantCulture);
                    builder.Add(analyzer);
                }
            }

            {
                var analyzer = new TooManyParameters();
                if (AnalyzerIds.Contains(analyzer.SupportedDiagnostics.Single().Id))
                {
                    analyzer.Maximum = int.Parse(
                        Parameters[analyzer.SupportedDiagnostics.Single().Id].Single()["max"],
                        NumberStyles.None, CultureInfo.InvariantCulture);
                    builder.Add(analyzer);
                }
            }

            {
                var analyzer = new ExpressionComplexity();
                if (AnalyzerIds.Contains(analyzer.SupportedDiagnostics.Single().Id))
                {
                    analyzer.Maximum = int.Parse(
                        Parameters[analyzer.SupportedDiagnostics.Single().Id].Single()["max"],
                        NumberStyles.None, CultureInfo.InvariantCulture);
                    builder.Add(analyzer);
                }
            }

            {
                var analyzer = new FunctionComplexity();
                if (AnalyzerIds.Contains(analyzer.SupportedDiagnostics.Single().Id))
                {
                    analyzer.Maximum = int.Parse(
                        Parameters[analyzer.SupportedDiagnostics.Single().Id].Single()["maximumFunctionComplexityThreshold"],
                        NumberStyles.None, CultureInfo.InvariantCulture);
                    builder.Add(analyzer);
                }
            }

            {
                var analyzer = new ClassName();
                if (AnalyzerIds.Contains(analyzer.SupportedDiagnostics.Single().Id))
                {
                    analyzer.Convention = Parameters[analyzer.SupportedDiagnostics.Single().Id].Single()["format"];
                    builder.Add(analyzer);
                }
            }

            {
                var analyzer = new MethodName();
                if (AnalyzerIds.Contains(analyzer.SupportedDiagnostics.Single().Id))
                {
                    analyzer.Convention = Parameters[analyzer.SupportedDiagnostics.Single().Id].Single()["format"];
                    builder.Add(analyzer);
                }
            }

            {
                var analyzer = new MagicNumber();
                if (AnalyzerIds.Contains(analyzer.SupportedDiagnostics.Single().Id))
                {
                    analyzer.Exceptions =
                        Parameters[analyzer.SupportedDiagnostics.Single().Id].Single()["exceptions"]
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToImmutableHashSet();
                    builder.Add(analyzer);
                }
            }

            var analyzedId = "S124";
            if (AnalyzerIds.Contains(analyzedId))
            {
                var rules = ImmutableArray.CreateBuilder<CommentRegularExpressionRule>();
                foreach (var parameters in Parameters[analyzedId])
                {
                    rules.Add(
                        new CommentRegularExpressionRule
                        {
                            // TODO: Add rule description
                            Descriptor = new DiagnosticDescriptor(parameters["RuleKey"], "TODO", parameters["message"], "SonarQube", DiagnosticSeverity.Warning, true),
                            RegularExpression = parameters["regularExpression"]
                        });
                }
                var analyzer = new CommentRegularExpression();
                analyzer.Rules = rules.ToImmutable();
                builder.Add(analyzer);
            }

            return builder.ToImmutable();
        }
    }
}
