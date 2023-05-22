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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCachedRegex : UseCachedRegexBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    private static readonly HashSet<SyntaxKind> CorrectContextSyntaxKinds = new()
    {
        SyntaxKind.MethodDeclaration,
        SyntaxKind.ConstructorDeclaration,
        SyntaxKind.DestructorDeclaration,
        SyntaxKind.GetAccessorDeclaration,
        SyntaxKind.SetAccessorDeclaration,
        SyntaxKindEx.LocalFunctionStatement,
        SyntaxKind.SimpleLambdaExpression,
        SyntaxKind.ParenthesizedLambdaExpression,
        SyntaxKind.AnonymousMethodExpression
    };

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var objectCreationExpression = (ObjectCreationExpressionSyntax)c.Node;

                if (objectCreationExpression.NameIs(nameof(Regex))
                    && IsWithinCorrectContext(objectCreationExpression)
                    && IsCorrectObjectType(objectCreationExpression, c.SemanticModel)
                    && IsArgumentConstantOrReadOnly(objectCreationExpression, c.SemanticModel)
                    && !IsCompliantAssignment(objectCreationExpression, c.SemanticModel))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, objectCreationExpression.GetLocation()));
                }
            },
            SyntaxKind.ObjectCreationExpression);

    private static bool IsWithinCorrectContext(SyntaxNode node)
    {
        while (!node.IsKind(SyntaxKind.CompilationUnit))
        {
            if (node.Parent.IsAnyKind(CorrectContextSyntaxKinds))
            {
                return true;
            }

            node = node.Parent;
        }

        return false;
    }

    private static bool IsCorrectObjectType(ObjectCreationExpressionSyntax objectCreationExpression, SemanticModel model) =>
        model.GetTypeInfo(objectCreationExpression).Type.Is(KnownType.System_Text_RegularExpressions_Regex);

    private static bool IsArgumentConstantOrReadOnly(ObjectCreationExpressionSyntax objectCreationExpression, SemanticModel model) =>
        objectCreationExpression.ArgumentList?.Arguments[0].Expression is { } expression
        && (model.GetConstantValue(expression).HasValue
            || model.GetSymbolInfo(expression).Symbol is IFieldSymbol { IsReadOnly: true });

    private static bool IsCompliantAssignment(ObjectCreationExpressionSyntax objectCreationExpression, SemanticModel model) =>
        objectCreationExpression.Parent switch
        {
            AssignmentExpressionSyntax assignment when assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) =>
                model.GetSymbolInfo(assignment.Left).Symbol is IFieldSymbol { IsReadOnly: true } or IPropertySymbol { IsReadOnly: true }
                || IsAssignmentWithinIfStatement(assignment, model),
            AssignmentExpressionSyntax assignment when assignment.IsKind(SyntaxKindEx.CoalesceAssignmentExpression) =>
                model.GetSymbolInfo(assignment.Left).Symbol is IFieldSymbol or IPropertySymbol,
            BinaryExpressionSyntax { Parent: AssignmentExpressionSyntax assignment } coalesce
                when coalesce.IsKind(SyntaxKind.CoalesceExpression) && assignment.IsAnyKind(SyntaxKind.SimpleAssignmentExpression, SyntaxKindEx.CoalesceAssignmentExpression)
                => coalesce.Left.GetName() == assignment.Left.GetName() && model.GetSymbolInfo(assignment.Left).Symbol is IFieldSymbol or IPropertySymbol,
            _ => false
        };

    private static bool IsAssignmentWithinIfStatement(AssignmentExpressionSyntax assignmentExpression, SemanticModel model)
    {
        SyntaxNode node = assignmentExpression;
        while (!node.Parent.IsKind(SyntaxKind.CompilationUnit))
        {
            if (node.Parent.IsKind(SyntaxKind.IfStatement))
            {
                break;
            }

            node = node.Parent;
        }

        return node.Parent is IfStatementSyntax { Condition: BinaryExpressionSyntax condition }
               && condition.IsAnyKind(SyntaxKind.EqualsExpression, SyntaxKind.IsExpression)
               && (ChecksForNullOnAssigned(condition.Left, condition.Right) || ChecksForNullOnAssigned(condition.Right, condition.Left))
               && model.GetSymbolInfo(assignmentExpression.Left).Symbol is IFieldSymbol or IPropertySymbol;

        bool ChecksForNullOnAssigned(ExpressionSyntax first, ExpressionSyntax second) =>
            assignmentExpression.Left.GetName() == first.GetName() && second is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NullLiteralExpression);
    }
}
