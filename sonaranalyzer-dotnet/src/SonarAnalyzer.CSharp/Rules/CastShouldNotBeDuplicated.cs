/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
    public class CastShouldNotBeDuplicated : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3247";
        private const string MessageFormat = "Replace this type-check-and-cast sequence with an 'as' and a null check.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(CheckForIssue, SyntaxKind.IsExpression);
        }

        private void CheckForIssue(SyntaxNodeAnalysisContext analysisContext)
        {
            var isExpression = (BinaryExpressionSyntax)analysisContext.Node;
            var castType = isExpression.Right as TypeSyntax;
            if (castType == null)
            {
                return;
            }

            var castTypeSymbol = analysisContext.SemanticModel.GetSymbolInfo(castType).Symbol as INamedTypeSymbol;
            if (castTypeSymbol == null ||
                castTypeSymbol.TypeKind == TypeKind.Struct)
            {
                return;
            }

            var parentIfStatement = isExpression.GetSelfOrTopParenthesizedExpression().Parent as IfStatementSyntax;
            if (parentIfStatement == null)
            {
                return;
            }

            var castExpressionWithCorrectType = parentIfStatement.Statement
                .DescendantNodes()
                .OfType<CastExpressionSyntax>()
                .FirstOrDefault(x => x.Type.IsEquivalentTo(castType));
            if (castExpressionWithCorrectType != null)
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(rule, isExpression.GetLocation(),
                    additionalLocations: new[] { castExpressionWithCorrectType.GetLocation() }));
            }
        }
    }
}
