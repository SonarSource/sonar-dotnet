/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class IssueSuppression : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1309";
        private const string MessageFormat = "Do not suppress issues.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
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
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, identifier.GetLocation()));
                    }
                },
                SyntaxKind.Attribute);

            context.RegisterSyntaxTreeActionInNonGenerated(
                c =>
                {
                    foreach (var token in c.Tree.GetRoot().DescendantTokens())
                    {
                        CheckTrivias(token.LeadingTrivia, c);
                        CheckTrivias(token.TrailingTrivia, c);
                    }
                });
        }

        private static void CheckTrivias(SyntaxTriviaList triviaList, SyntaxTreeAnalysisContext c)
        {
            var pragmaWarnings = triviaList
                .Where(t => t.HasStructure)
                .Select(t => t.GetStructure())
                .OfType<PragmaWarningDirectiveTriviaSyntax>()
                .Where(t => t.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword));

            foreach (var pragmaWarning in pragmaWarnings)
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule,
                    pragmaWarning.CreateLocation(pragmaWarning.DisableOrRestoreKeyword)));
            }
        }
    }
}
