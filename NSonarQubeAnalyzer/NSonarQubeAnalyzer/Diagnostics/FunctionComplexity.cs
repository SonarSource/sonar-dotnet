using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Threading;
using System.Text.RegularExpressions;

namespace NSonarQubeAnalyzer
{
    using System.Xml.Linq;

    [Serializable]
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FunctionComplexity : DiagnosticsRule
    {
        internal const string DiagnosticId = "FunctionComplexity";
        internal const string Description = "Method complexity should not be too high";
        internal const string MessageFormat = "Refactor this method that has a complexity of {1} (which is greater than {0} authorized).";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public int Maximum;

        public override string RuleId
        {
            get
            {
                return "FunctionComplexity";
            }
        }

        public override void SetDefaultSettings()
        {
            this.Maximum = 10;
        }

        public override void UpdateParameters(Dictionary<string, string> parameters)
        {
            this.Maximum = int.Parse(parameters["maximumFunctionComplexityThreshold"]);
        }

        public override void Configure(XDocument settings)
        {
            var parameters = from e in settings.Descendants("Rule")
                             where this.RuleId.Equals(e.Elements("Key").Single().Value)
                             select e.Descendants("Parameter");
            var maximum = (from e in parameters
                           where "maximumFunctionComplexityThreshold".Equals(e.Elements("Key").Single().Value)
                           select e.Elements("Value").Single().Value).Single();

            this.Maximum = int.Parse(maximum);
        }

        public override void Initialize(AnalysisContext context)
        {
            if (!Status)
            {
                return;
            }

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var complexity = Metrics.Complexity(c.Node);
                    if (complexity > Maximum)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, c.Node.GetLocation(), Maximum, complexity));
                    }
                },
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.OperatorDeclaration,
                SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration);
        }
    }
}
