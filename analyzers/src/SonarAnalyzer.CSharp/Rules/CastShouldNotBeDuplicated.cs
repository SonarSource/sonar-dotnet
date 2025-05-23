﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CastShouldNotBeDuplicated : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3247";
    private const string MessageFormat = "{0}";
    private const string UsePatternMatchingCheckMessage = "Replace this type-check-and-cast sequence to use pattern matching.";
    private const string RemoveRedundantCastAnotherVariableMessage = "Remove this cast and use the appropriate variable.";
    private const string RemoveRedundantCastMessage = "Remove this redundant cast.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(IsExpression, SyntaxKind.IsExpression);
        context.RegisterNodeAction(IsPatternExpression, SyntaxKindEx.IsPatternExpression);
        context.RegisterNodeAction(SwitchExpressionArm, SyntaxKindEx.SwitchExpressionArm);
        context.RegisterNodeAction(CasePatternSwitchLabel, SyntaxKindEx.CasePatternSwitchLabel);
    }

    private static void CasePatternSwitchLabel(SonarSyntaxNodeReportingContext analysisContext)
    {
        var casePatternSwitch = (CasePatternSwitchLabelSyntaxWrapper)analysisContext.Node;
        if (casePatternSwitch.SyntaxNode.GetFirstNonParenthesizedParent().GetFirstNonParenthesizedParent() is SwitchStatementSyntax parentSwitchStatement)
        {
            ProcessPatternExpression(analysisContext, casePatternSwitch.Pattern, parentSwitchStatement.Expression, parentSwitchStatement);
        }
    }

    private static void SwitchExpressionArm(SonarSyntaxNodeReportingContext analysisContext)
    {
        var isSwitchExpression = (SwitchExpressionArmSyntaxWrapper)analysisContext.Node;
        var parent = isSwitchExpression.SyntaxNode.GetFirstNonParenthesizedParent();
        if (parent.IsKind(SyntaxKindEx.SwitchExpression))
        {
            var switchExpression = (SwitchExpressionSyntaxWrapper)parent;
            ProcessPatternExpression(analysisContext, isSwitchExpression.Pattern, switchExpression.GoverningExpression, isSwitchExpression);
        }
    }

    private static void IsPatternExpression(SonarSyntaxNodeReportingContext analysisContext)
    {
        var isPatternExpression = (IsPatternExpressionSyntaxWrapper)analysisContext.Node;
        if (isPatternExpression.SyntaxNode.GetFirstNonParenthesizedParent() is IfStatementSyntax parentIfStatement)
        {
            ProcessPatternExpression(analysisContext, isPatternExpression.Pattern, isPatternExpression.Expression, parentIfStatement.Statement);
        }
    }

    private static void IsExpression(SonarSyntaxNodeReportingContext analysisContext)
    {
        var isExpression = (BinaryExpressionSyntax)analysisContext.Node;
        if (isExpression.Right is TypeSyntax castType
            && isExpression.GetFirstNonParenthesizedParent() is IfStatementSyntax parentIfStatement)
        {
            ReportPatternAtMainVariable(analysisContext, isExpression.Left, isExpression.GetLocation(), parentIfStatement.Statement, castType, UsePatternMatchingCheckMessage);
        }
    }

    private static Location[] DuplicatedCastLocations(SonarSyntaxNodeReportingContext context, SyntaxNode parentStatement, TypeSyntax castType, SyntaxNode typedVariable)
    {
        var typeExpressionSymbol = context.SemanticModel.GetSymbolInfo(typedVariable).Symbol ?? context.SemanticModel.GetDeclaredSymbol(typedVariable);
        return typeExpressionSymbol is null ? [] : parentStatement.DescendantNodes().Where(IsDuplicatedCast).Select(x => x.GetLocation()).ToArray();

        bool IsDuplicatedCast(SyntaxNode node)
        {
            if (node is CastExpressionSyntax cast)
            {
                return IsDuplicatedCastOnSameSymbol(cast.Expression, cast.Type);
            }
            else if (node is BinaryExpressionSyntax binary && binary.IsAnyKind(SyntaxKind.AsExpression, SyntaxKind.IsExpression))
            {
                return IsDuplicatedCastOnSameSymbol(binary.Left, binary.Right);
            }
            else
            {
                return false;
            }
        }

        bool IsDuplicatedCastOnSameSymbol(ExpressionSyntax expression, SyntaxNode type) =>
            type.WithoutTrivia().IsEquivalentTo(castType.WithoutTrivia())
            && IsCastOnSameSymbol(expression)
            && !CSharpFacade.Instance.Syntax.IsInExpressionTree(context.SemanticModel, expression); // see https://github.com/SonarSource/sonar-dotnet/issues/8735#issuecomment-1943419398

        bool IsCastOnSameSymbol(ExpressionSyntax expression) =>
            IsEquivalentVariable(expression, typedVariable)
            && Equals(context.SemanticModel.GetSymbolInfo(expression).Symbol, typeExpressionSymbol);
    }

    private static void ProcessPatternExpression(SonarSyntaxNodeReportingContext analysisContext, SyntaxNode isPattern, SyntaxNode mainVariableExpression, SyntaxNode parentStatement)
    {
        var objectToPattern = new Dictionary<ExpressionSyntax, SyntaxNode>();
        PatternExpressionObjectToPatternMapping.MapObjectToPattern((ExpressionSyntax)mainVariableExpression.RemoveParentheses(), isPattern.RemoveParentheses(), objectToPattern);
        foreach (var expressionPatternPair in objectToPattern)
        {
            var pattern = expressionPatternPair.Value;
            var leftVariable = expressionPatternPair.Key;
            var targetTypes = GetTypesFromPattern(pattern);
            var rightPartsToCheck = new Dictionary<SyntaxNode, Tuple<TypeSyntax, Location>>();
            foreach (var subPattern in pattern.DescendantNodesAndSelf().Where(x => x.IsAnyKind(SyntaxKindEx.DeclarationPattern, SyntaxKindEx.RecursivePattern)))
            {
                if (DeclarationPatternSyntaxWrapper.IsInstance(subPattern) && (DeclarationPatternSyntaxWrapper)subPattern is var declarationPattern)
                {
                    rightPartsToCheck.Add(declarationPattern.Designation.SyntaxNode, new Tuple<TypeSyntax, Location>(declarationPattern.Type, subPattern.GetLocation()));
                }
                else if ((RecursivePatternSyntaxWrapper)subPattern is { Designation.SyntaxNode: { }, Type: { } } recursivePattern)
                {
                    rightPartsToCheck.Add(recursivePattern.Designation.SyntaxNode, new Tuple<TypeSyntax, Location>(recursivePattern.Type, subPattern.GetLocation()));
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
                ReportPatternAtCastLocation(analysisContext, variableTypePair.Key, variableTypePair.Value.Item2, parentStatement, variableTypePair.Value.Item1, RemoveRedundantCastMessage);
            }
        }
    }

    private static IEnumerable<TypeSyntax> GetTypesFromPattern(SyntaxNode pattern)
    {
        var targetTypes = new HashSet<TypeSyntax>();
        if (RecursivePatternSyntaxWrapper.IsInstance(pattern) && ((RecursivePatternSyntaxWrapper)pattern is { PositionalPatternClause.SyntaxNode: { } } recursivePattern))
        {
            foreach (var subpattern in recursivePattern.PositionalPatternClause.Subpatterns)
            {
                AddPatternType(subpattern.Pattern, targetTypes);
            }
        }
        else if (BinaryPatternSyntaxWrapper.IsInstance(pattern) && (BinaryPatternSyntaxWrapper)pattern is { } binaryPattern)
        {
            AddPatternType(binaryPattern.Left, targetTypes);
            AddPatternType(binaryPattern.Right, targetTypes);
        }
        else if (ListPatternSyntaxWrapper.IsInstance(pattern) && (ListPatternSyntaxWrapper)pattern is { } listPattern)
        {
            foreach (var subpattern in listPattern.Patterns)
            {
                AddPatternType(subpattern, targetTypes);
            }
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

    private static void ReportPatternAtMainVariable(SonarSyntaxNodeReportingContext context,
                                                    SyntaxNode variableExpression,
                                                    Location mainLocation,
                                                    SyntaxNode parentStatement,
                                                    TypeSyntax castType,
                                                    string message)
    {
        var duplicatedCastLocations = DuplicatedCastLocations(context, parentStatement, castType, variableExpression);
        if (duplicatedCastLocations.Any())
        {
            context.ReportIssue(Rule, mainLocation, duplicatedCastLocations.ToSecondary(), message);
        }
    }

    private static void ReportPatternAtCastLocation(SonarSyntaxNodeReportingContext context,
                                                    SyntaxNode variableExpression,
                                                    Location patternLocation,
                                                    SyntaxNode parentStatement,
                                                    TypeSyntax castType,
                                                    string message)
    {
        var duplicatedCastLocations = DuplicatedCastLocations(context, parentStatement, castType, variableExpression);
        foreach (var castLocation in duplicatedCastLocations)
        {
            context.ReportIssue(Rule, castLocation, [patternLocation.ToSecondary()], message);
        }
    }

    private static bool IsEquivalentVariable(ExpressionSyntax expression, SyntaxNode typedVariable)
    {
        var left = CleanupExpression(typedVariable).WithoutTrivia();
        var right = CleanupExpression(expression).WithoutTrivia();

        return left.IsEquivalentTo(right)
            || (StandaloneIdentifier(left) is { } leftIdentifier && leftIdentifier == StandaloneIdentifier(right));

        static string StandaloneIdentifier(SyntaxNode node) =>
            node switch
            {
                IdentifierNameSyntax name => name.Identifier.ValueText,
                _ when node.IsKind(SyntaxKindEx.SingleVariableDesignation) => ((SingleVariableDesignationSyntaxWrapper)node).Identifier.ValueText,
                _ => null
            };
    }

    private static SyntaxNode CleanupExpression(SyntaxNode node)
    {
        while (node is ParenthesizedExpressionSyntax parenthesized)
        {
            node = parenthesized.Expression;
        }
        return node is MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } memberAccess ? memberAccess.Name : node;
    }
}
