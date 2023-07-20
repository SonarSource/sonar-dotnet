/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class IssueSuppression : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1309";
        private const string MessageFormat = "Do not suppress issues.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var attribute = (AttributeSyntax)c.Node;

                    if (!(c.SemanticModel.GetSymbolInfo(attribute).Symbol is IMethodSymbol attributeConstructor) ||
                        !attributeConstructor.ContainingType.Is(KnownType.System_Diagnostics_CodeAnalysis_SuppressMessageAttribute))
                    {
                        return;
                    }

                    if (!(attribute.Name is IdentifierNameSyntax identifier))
                    {
                        identifier = (attribute.Name as QualifiedNameSyntax)?.Right as IdentifierNameSyntax;
                    }

                    if (identifier != null)
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, identifier.GetLocation()));
                    }
                },
                SyntaxKind.Attribute);

            context.RegisterTreeAction(
                c =>
                {
                    foreach (var token in c.Tree.GetRoot().DescendantTokens())
                    {
                        CheckTrivias(c, token.LeadingTrivia);
                        CheckTrivias(c, token.TrailingTrivia);
                    }
                });
        }

        private static void CheckTrivias(SonarSyntaxTreeReportingContext c, SyntaxTriviaList triviaList)
        {
            var pragmaWarnings = triviaList
                .Where(t => t.HasStructure)
                .Select(t => t.GetStructure())
                .OfType<PragmaWarningDirectiveTriviaSyntax>()
                .Where(t => t.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword));

            foreach (var pragmaWarning in pragmaWarnings)
            {
                c.ReportIssue(CreateDiagnostic(Rule, pragmaWarning.CreateLocation(pragmaWarning.DisableOrRestoreKeyword)));
            }
        }
    }
}
