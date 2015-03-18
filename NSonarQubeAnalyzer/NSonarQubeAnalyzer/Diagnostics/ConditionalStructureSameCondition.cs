using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NSonarQubeAnalyzer.Diagnostics
{
    public class ConditionalStructureSameCondition : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1862";
        internal const string Description = @"Related ""if/else if"" statements should not have the same condition";
        internal const string MessageFormat = @"This branch duplicates the one on line {0}.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var ifStatement = c.Node as IfStatementSyntax;

                    if (ifStatement == null)
                    {
                        return;
                    }

                    var currentCondition = ifStatement.Condition;

                    var preceedingIfs = GetPreceedingIfs(ifStatement);
                    var preceedingConditions = preceedingIfs.Select(st => st.Condition).ToList();

                    for (int i = 0; i < preceedingConditions.Count; i++)
                    {
                        var preceedingCondition = preceedingConditions[i];

                        if (currentCondition.IsEquivalentTo(preceedingCondition))
                        {
                            c.ReportDiagnostic(Diagnostic.Create(
                                Rule,
                                currentCondition.GetLocation(),
                                preceedingCondition.GetLocation().GetLineSpan().StartLinePosition.Line + 1));
                            break;
                        }
                    }
                },
                SyntaxKind.IfStatement);
        }

        private List<IfStatementSyntax> GetPreceedingIfs(IfStatementSyntax ifStatement)
        {
            var ifList = new List<IfStatementSyntax>();
            var currentIf = ifStatement;

            while (currentIf.Parent is ElseClauseSyntax)
            {
                var preceedingIf = currentIf.Parent.Parent as IfStatementSyntax;
                if (preceedingIf == null)
                {
                    break;
                }

                ifList.Add(preceedingIf);
                currentIf = preceedingIf;
            }

            ifList.Reverse();
            return ifList;
        }
    }
}
