/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CastShouldNotBeDuplicated : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3247";
        private const string MessageFormat = "{0}";
        private const string ReplaceWithAsAndNullCheckMessage = "Replace this type-check-and-cast sequence with an 'as' and a null check.";
        private const string RemoveRedundantCaseMessage = "Remove this redundant cast.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(IsExpression, SyntaxKind.IsExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(IsPatternExpression, SyntaxKindEx.IsPatternExpression);
        }

        private static void IsPatternExpression(SyntaxNodeAnalysisContext analysisContext)
        {
            var isPatternExpression = (IsPatternExpressionSyntaxWrapper)analysisContext.Node;
            if (!(isPatternExpression.SyntaxNode.GetFirstNonParenthesizedParent() is IfStatementSyntax parentIfStatement))
            {
                return;
            }
            var isPatternLocation = isPatternExpression.SyntaxNode.GetLocation();
            if (isPatternExpression.Pattern.SyntaxNode.IsKind(SyntaxKindEx.RecursivePattern))
            {
                var recursivePattern = (RecursivePatternSyntaxWrapper)isPatternExpression.Pattern.SyntaxNode;
                for (var i = 0; i < recursivePattern.PositionalPatternClause.Subpatterns.Count; i++)
                {
                    var pattern = recursivePattern.PositionalPatternClause.Subpatterns[i].Pattern;
                    if (ProcessPattern(analysisContext, isPatternExpression, pattern, parentIfStatement) is { } patternType
                        && isPatternExpression.Expression.IsKind(SyntaxKindEx.TupleExpression)
                        && (TupleExpressionSyntaxWrapper)isPatternExpression.Expression is var tupleExpression
                        && tupleExpression.Arguments.Count == recursivePattern.PositionalPatternClause.Subpatterns.Count)
                    {
                        ReportIsExpressionLeftPartDuplicateCast(analysisContext, tupleExpression.Arguments[i].Expression, isPatternLocation, parentIfStatement, patternType);
                    }
                }
            }
            else if (ProcessPattern(analysisContext, isPatternExpression, isPatternExpression.Pattern, parentIfStatement) is { } patternType)
            {
                ReportIsExpressionLeftPartDuplicateCast(analysisContext, isPatternExpression.Expression, isPatternLocation, parentIfStatement, patternType);
            }
        }

        private static void IsExpression(SyntaxNodeAnalysisContext analysisContext)
        {
            var isExpression = (BinaryExpressionSyntax)analysisContext.Node;

            if (!(isExpression.Right is TypeSyntax castType) ||
                !(isExpression.GetFirstNonParenthesizedParent() is IfStatementSyntax parentIfStatement) ||
                !(analysisContext.SemanticModel.GetSymbolInfo(castType).Symbol is INamedTypeSymbol castTypeSymbol) ||
                castTypeSymbol.TypeKind == TypeKind.Struct)
            {
                return;
            }

            ReportIsExpressionLeftPartDuplicateCast(analysisContext, isExpression.Left, isExpression.GetLocation(), parentIfStatement, castType);
        }

        private static void ReportIsExpressionLeftPartDuplicateCast(SyntaxNodeAnalysisContext analysisContext,
                                                                    SyntaxNode leftExpression,
                                                                    Location isExpressionLocation,
                                                                    IfStatementSyntax parentIfStatement,
                                                                    TypeSyntax castType)
        {
            var duplicatedCastLocations = GetDuplicatedCastLocations(analysisContext, parentIfStatement, castType, leftExpression);

            if (duplicatedCastLocations.Any())
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, isExpressionLocation, duplicatedCastLocations, ReplaceWithAsAndNullCheckMessage));
            }
        }

        private static List<Location> GetDuplicatedCastLocations(SyntaxNodeAnalysisContext analysisContext, IfStatementSyntax parentIfStatement, TypeSyntax castType, SyntaxNode typedVariable)
        {
            var typeExpressionSymbol = analysisContext.SemanticModel.GetSymbolInfo(typedVariable).Symbol
                                       ?? analysisContext.SemanticModel.GetDeclaredSymbol(typedVariable);
            if (typeExpressionSymbol == null)
            {
                return new List<Location>();
            }

            return parentIfStatement.Statement
                .DescendantNodes()
                .OfType<CastExpressionSyntax>()
                .Where(x => x.Type is IdentifierNameSyntax identifierNameSyntax
                            && identifierNameSyntax.Identifier.ValueText == castType.GetName()
                            && IsCastOnSameSymbol(x))
                .Select(x => x.GetLocation()).ToList();

            bool IsCastOnSameSymbol(CastExpressionSyntax castExpression) =>
                Equals(analysisContext.SemanticModel.GetSymbolInfo(castExpression.Expression).Symbol, typeExpressionSymbol);
        }


        private static TypeSyntax ProcessPattern(SyntaxNodeAnalysisContext analysisContext, SyntaxNode isPatternExpression, SyntaxNode pattern, IfStatementSyntax parentIfStatement)
        {
            if (pattern.IsKind(SyntaxKindEx.DeclarationPattern)
                && (DeclarationPatternSyntaxWrapper)pattern is var declarationPattern
                && declarationPattern.Designation.SyntaxNode.IsKind(SyntaxKindEx.SingleVariableDesignation)
                && (SingleVariableDesignationSyntaxWrapper)declarationPattern.Designation.SyntaxNode is var singleVariableDesignation)
            {
                if (!(analysisContext.SemanticModel.GetSymbolInfo(declarationPattern.Type).Symbol is INamedTypeSymbol castTypeSymbol)
                    || castTypeSymbol.TypeKind == TypeKind.Struct)
                {
                    return declarationPattern.Type;
                }

                var isPatternLocation = isPatternExpression.GetLocation();
                var duplicatedCastLocations = GetDuplicatedCastLocations(analysisContext, parentIfStatement, declarationPattern.Type, singleVariableDesignation.SyntaxNode);
                foreach (var castLocation in duplicatedCastLocations)
                {
                    analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, castLocation, new[] { isPatternLocation }, RemoveRedundantCaseMessage));
                }

                return declarationPattern.Type;
            }
            return null;
        }
    }
}
