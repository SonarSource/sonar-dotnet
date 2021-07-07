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
using SonarAnalyzer.Extensions;
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
        private const string RemoveRedundantCastAnotherVariableMessage = "Remove this cast and use the appropriate variable.";
        private const string RemoveRedundantCastMessage = "Remove this redundant cast.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(IsExpression, SyntaxKind.IsExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(IsPatternExpression, SyntaxKindEx.IsPatternExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(SwitchExpressionArm, SyntaxKindEx.SwitchExpressionArm);
            context.RegisterSyntaxNodeActionInNonGenerated(CasePatternSwitchLabel, SyntaxKindEx.CasePatternSwitchLabel);
        }

        private static void CasePatternSwitchLabel(SyntaxNodeAnalysisContext analysisContext)
        {
            var casePatternSwitch = (CasePatternSwitchLabelSyntaxWrapper)analysisContext.Node;
            if (!(casePatternSwitch.SyntaxNode.GetFirstNonParenthesizedParent().GetFirstNonParenthesizedParent() is SwitchStatementSyntax parentSwitchStatement))
            {
                return;
            }
            ProcessPatternExpression(analysisContext,
                                     casePatternSwitch.Pattern,
                                     parentSwitchStatement.Expression,
                                     parentSwitchStatement);
        }

        private static void SwitchExpressionArm(SyntaxNodeAnalysisContext analysisContext)
        {
            var isSwitchExpression = (SwitchExpressionArmSyntaxWrapper)analysisContext.Node;
            var parent = isSwitchExpression.SyntaxNode.GetFirstNonParenthesizedParent();

            if (!parent.IsKind(SyntaxKindEx.SwitchExpression))
            {
                return;
            }
            var switchExpression = (SwitchExpressionSyntaxWrapper)parent;
            ProcessPatternExpression(analysisContext,
                                     isSwitchExpression.Pattern,
                                     switchExpression.GoverningExpression,
                                     isSwitchExpression);
        }

        private static void IsPatternExpression(SyntaxNodeAnalysisContext analysisContext)
        {
            var isPatternExpression = (IsPatternExpressionSyntaxWrapper)analysisContext.Node;
            if (!(isPatternExpression.SyntaxNode.GetFirstNonParenthesizedParent() is IfStatementSyntax parentIfStatement))
            {
                return;
            }
            ProcessPatternExpression(analysisContext,
                                     isPatternExpression.Pattern,
                                     isPatternExpression.Expression,
                                     parentIfStatement.Statement);
        }

        private static void IsExpression(SyntaxNodeAnalysisContext analysisContext)
        {
            var isExpression = (BinaryExpressionSyntax)analysisContext.Node;

            if (!(isExpression.Right is TypeSyntax castType)
                || !(isExpression.GetFirstNonParenthesizedParent() is IfStatementSyntax parentIfStatement)
                || !(analysisContext.SemanticModel.GetSymbolInfo(castType).Symbol is INamedTypeSymbol castTypeSymbol)
                || castTypeSymbol.TypeKind == TypeKind.Struct)
            {
                return;
            }

            ReportPatternAtMainVariable(analysisContext, isExpression.Left, isExpression.GetLocation(), parentIfStatement.Statement, castType, ReplaceWithAsAndNullCheckMessage);
        }

        private static List<Location> GetDuplicatedCastLocations(SyntaxNodeAnalysisContext analysisContext, SyntaxNode parentStatement, TypeSyntax castType, SyntaxNode typedVariable)
        {
            var typeExpressionSymbol = analysisContext.SemanticModel.GetSymbolInfo(typedVariable).Symbol
                                       ?? analysisContext.SemanticModel.GetDeclaredSymbol(typedVariable);
            if (typeExpressionSymbol == null)
            {
                return new List<Location>();
            }

            return parentStatement
                .DescendantNodes()
                .OfType<CastExpressionSyntax>()
                .Where(x => x.Type.WithoutTrivia().IsEquivalentTo(castType.WithoutTrivia())
                            && IsCastOnSameSymbol(x))
                .Select(x => x.GetLocation()).ToList();

            bool IsCastOnSameSymbol(CastExpressionSyntax castExpression) =>
                Equals(analysisContext.SemanticModel.GetSymbolInfo(castExpression.Expression).Symbol, typeExpressionSymbol);
        }

        private static void ProcessPatternExpression(SyntaxNodeAnalysisContext analysisContext,
                                                      SyntaxNode isPattern,
                                                      SyntaxNode mainVariableExpression,
                                                      SyntaxNode parentStatement)
        {
            var objectToPattern = new Dictionary<ExpressionSyntax, SyntaxNode>();
            MapObjectToPattern((ExpressionSyntax)mainVariableExpression.RemoveParentheses(), isPattern.RemoveParentheses(), objectToPattern);
            foreach (var expressionPatternPair in objectToPattern)
            {
                var pattern = expressionPatternPair.Value.RemoveParentheses();
                var leftVariable = expressionPatternPair.Key;
                var targetTypes = GetTypesFromPattern(pattern);
                var rightPartsToCheck = new Dictionary<SyntaxNode, TypeSyntax>();
                foreach (var subPattern in pattern.DescendantNodesAndSelf().Where(x => x.IsAnyKind(SyntaxKindEx.DeclarationPattern, SyntaxKindEx.RecursivePattern)))
                {
                    if (DeclarationPatternSyntaxWrapper.IsInstance(subPattern) && (DeclarationPatternSyntaxWrapper)subPattern is var declarationPattern)
                    {
                        rightPartsToCheck.Add(declarationPattern.Designation.SyntaxNode, declarationPattern.Type);
                    }
                    else if ((RecursivePatternSyntaxWrapper)subPattern is {Designation: {SyntaxNode: { }}, Type: { }} recursivePattern)
                    {
                        rightPartsToCheck.Add(recursivePattern.Designation.SyntaxNode, recursivePattern.Type);
                    }
                }

                var mainVarMsg = rightPartsToCheck.Any()
                                 ? RemoveRedundantCastAnotherVariableMessage
                                 : RemoveRedundantCastMessage;
                foreach (var targetType in targetTypes)
                {
                    ReportPatternAtMainVariable(analysisContext, leftVariable, leftVariable.GetLocation(), parentStatement, targetType, mainVarMsg);
                }

                foreach (var variableTypePair in rightPartsToCheck)
                {
                    ReportPatternAtCastLocation(analysisContext, variableTypePair.Key, pattern.GetLocation(), parentStatement, variableTypePair.Value, RemoveRedundantCastMessage);
                }
            }
        }

        private static IEnumerable<TypeSyntax> GetTypesFromPattern(SyntaxNode pattern)
        {
            var targetTypes = new HashSet<TypeSyntax>();
            if (RecursivePatternSyntaxWrapper.IsInstance(pattern) && ((RecursivePatternSyntaxWrapper)pattern is {PositionalPatternClause: {SyntaxNode: {}}} recursivePattern))
            {
                foreach (var subpattern in recursivePattern.PositionalPatternClause.Subpatterns)
                {
                    AddPatternType(subpattern.Pattern, targetTypes);
                }
            }
            else if (BinaryPatternSyntaxWrapper.IsInstance(pattern) && (BinaryPatternSyntaxWrapper)pattern is {} binaryPattern)
            {
                AddPatternType(binaryPattern.Left, targetTypes);
                AddPatternType(binaryPattern.Right, targetTypes);
            }
            else
            {
                AddPatternType(pattern, targetTypes);
            }
            return targetTypes;

            static void AddPatternType(SyntaxNode pattern, ISet<TypeSyntax> targetTypes)
            {
                if (GetType(pattern) is { } patternType)
                {
                    targetTypes.Add(patternType);
                }
            }
        }

        private static TypeSyntax GetType(SyntaxNode pattern)
        {
            if (ConstantPatternSyntaxWrapper.IsInstance(pattern))
            {
                return ((ConstantPatternSyntaxWrapper)pattern).Expression as TypeSyntax;
            }
            else if (DeclarationPatternSyntaxWrapper.IsInstance(pattern))
            {
                return ((DeclarationPatternSyntaxWrapper)pattern).Type;
            }
            else if (RecursivePatternSyntaxWrapper.IsInstance(pattern))
            {
                return ((RecursivePatternSyntaxWrapper)pattern).Type;
            }
            return null;
        }

        private static void MapObjectToPattern(ExpressionSyntax expression, SyntaxNode pattern, IDictionary<ExpressionSyntax, SyntaxNode> objectToPatternMap)
        {
            if (TupleExpressionSyntaxWrapper.IsInstance(expression) && ((TupleExpressionSyntaxWrapper)expression) is var tupleExpression)
            {
                if (!RecursivePatternSyntaxWrapper.IsInstance(pattern) || ((RecursivePatternSyntaxWrapper)pattern is {PositionalPatternClause: {SyntaxNode: null}} recursivePattern))
                {
                    return;
                }

                for (var i = 0; i < tupleExpression.Arguments.Count; i++)
                {
                    MapObjectToPattern(tupleExpression.Arguments[i].Expression.RemoveParentheses(),
                                       recursivePattern.PositionalPatternClause.Subpatterns[i].Pattern.RemoveParentheses(),
                                       objectToPatternMap);
                }
            }
            else
            {
                objectToPatternMap.Add(expression, pattern);
            }
        }

        private static void ReportPatternAtMainVariable(SyntaxNodeAnalysisContext analysisContext,
                                                        SyntaxNode variableExpression,
                                                        Location mainLocation,
                                                        SyntaxNode parentStatement,
                                                        TypeSyntax castType,
                                                        string message)
        {
            var duplicatedCastLocations = GetDuplicatedCastLocations(analysisContext, parentStatement, castType, variableExpression);

            if (duplicatedCastLocations.Any())
            {
                analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, mainLocation, duplicatedCastLocations, message));
            }
        }

        private static void ReportPatternAtCastLocation(SyntaxNodeAnalysisContext analysisContext,
                                                        SyntaxNode variableExpression,
                                                        Location patternLocation,
                                                        SyntaxNode parentStatement,
                                                        TypeSyntax castType,
                                                        string message)
        {
            if (analysisContext.SemanticModel.GetSymbolInfo(castType).Symbol is INamedTypeSymbol castTypeSymbol
                && castTypeSymbol.TypeKind != TypeKind.Struct)
            {
                var duplicatedCastLocations = GetDuplicatedCastLocations(analysisContext, parentStatement, castType, variableExpression);
                foreach (var castLocation in duplicatedCastLocations)
                {
                    analysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, castLocation, new[] { patternLocation }, message));
                }
            }
        }
    }
}
