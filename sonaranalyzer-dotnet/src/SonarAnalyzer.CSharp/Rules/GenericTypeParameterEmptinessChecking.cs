/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class GenericTypeParameterEmptinessChecking : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2955";
        private const string MessageFormat =
            "Use a comparison to 'default({0})' instead or add a constraint to '{0}' so that it can't be a value type.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var equalsExpression = (BinaryExpressionSyntax) c.Node;

                    var leftIsNull = CSharpEquivalenceChecker.AreEquivalent(equalsExpression.Left, CSharpSyntaxHelper.NullLiteralExpression);
                    var rightIsNull = CSharpEquivalenceChecker.AreEquivalent(equalsExpression.Right, CSharpSyntaxHelper.NullLiteralExpression);

                    if (!(leftIsNull ^ rightIsNull))
                    {
                        return;
                    }

                    var expressionToTypeCheck = leftIsNull ? equalsExpression.Right : equalsExpression.Left;
                    if (c.SemanticModel.GetTypeInfo(expressionToTypeCheck).Type is ITypeParameterSymbol typeInfo &&
                        !typeInfo.HasReferenceTypeConstraint &&
                        !typeInfo.ConstraintTypes.OfType<IErrorTypeSymbol>().Any() &&
                        !typeInfo.ConstraintTypes.Any(typeSymbol =>
                            typeSymbol.IsReferenceType &&
                            typeSymbol.IsClass()))
                    {
                        var expressionToReportOn = leftIsNull ? equalsExpression.Left : equalsExpression.Right;

                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, expressionToReportOn.GetLocation(),
                            typeInfo.Name));
                    }
                },
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);
        }
    }
}
