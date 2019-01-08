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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.Common;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class ShouldImplementExportedInterfaces :
        ShouldImplementExportedInterfacesBase<ArgumentSyntax, ExpressionSyntax, ClassStatementSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var attributeSyntax = (AttributeSyntax)c.Node;

                    if (!(c.SemanticModel.GetSymbolInfo(attributeSyntax.Name).Symbol is IMethodSymbol   attributeCtorSymbol) ||
                        !attributeCtorSymbol.ContainingType.IsAny(ExportAttributes))
                    {
                        return;
                    }

                    var exportedType = GetExportedTypeSymbol(attributeSyntax.ArgumentList?.Arguments, c.SemanticModel);
                    var attributeTargetType = GetAttributeTargetSymbol(attributeSyntax, c.SemanticModel);

                    if (exportedType != null &&
                        attributeTargetType != null &&
                        !IsOfExportType(attributeTargetType, exportedType))
                    {
                        var action = exportedType.IsInterface()
                            ? ActionForInterface
                            : ActionForClass;

                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, attributeSyntax.GetLocation(), action,
                            exportedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                            attributeTargetType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
                    }
                },
                SyntaxKind.Attribute);
        }

        protected override string GetIdentifier(ArgumentSyntax argumentSyntax) =>
            (argumentSyntax as SimpleArgumentSyntax)?.NameColonEquals?.Name.Identifier.ValueText;
        protected override ExpressionSyntax GetExpression(ArgumentSyntax argumentSyntax) =>
            argumentSyntax?.GetExpression();
        protected override SyntaxNode GetTypeOfOrGetTypeExpression(ExpressionSyntax expressionSyntax) =>
            (expressionSyntax as GetTypeExpressionSyntax)?.Type;
    }
}
