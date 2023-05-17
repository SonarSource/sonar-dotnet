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

    private static bool IsCompliantAssignment(ObjectCreationExpressionSyntax objectCreationExpression, SemanticModel model) =>
        objectCreationExpression.Parent switch
        {
            AssignmentExpressionSyntax { RawKind: (int)SyntaxKind.SimpleAssignmentExpression } assignmentExpression =>
                model.GetSymbolInfo(assignmentExpression.Left).Symbol is IFieldSymbol { IsReadOnly: true } or IPropertySymbol { IsReadOnly: true }
                || IsAssignmentWithinIfStatement(assignmentExpression, model),
            AssignmentExpressionSyntax { RawKind: (int)SyntaxKindEx.CoalesceAssignmentExpression } assignmentExpression =>
                model.GetSymbolInfo(assignmentExpression.Left).Symbol is IFieldSymbol or IPropertySymbol,
            BinaryExpressionSyntax
            {
                RawKind: (int)SyntaxKind.CoalesceExpression,
                Parent: AssignmentExpressionSyntax { RawKind: (int)SyntaxKind.SimpleAssignmentExpression or (int)SyntaxKindEx.CoalesceAssignmentExpression } assignmentExpression
            } coalesceExpression => coalesceExpression.Left.GetName() == assignmentExpression.Left.GetName()
                                    && model.GetSymbolInfo(assignmentExpression.Left).Symbol is IFieldSymbol or IPropertySymbol,
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

        return node.Parent is IfStatementSyntax { Condition: BinaryExpressionSyntax { RawKind: (int)SyntaxKind.EqualsExpression or (int)SyntaxKind.IsExpression } condition }
               && ((assignmentExpression.Left.GetName() == condition.Left.GetName()
                    && condition.Right is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression })
                   || (assignmentExpression.Left.GetName() == condition.Right.GetName()
                       && condition.Left is LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression }))
               && model.GetSymbolInfo(assignmentExpression.Left).Symbol is IFieldSymbol or IPropertySymbol;
    }

    private static bool IsCorrectObjectType(ObjectCreationExpressionSyntax objectCreationExpression, SemanticModel model) =>
        model.GetTypeInfo(objectCreationExpression).Type.Is(KnownType.System_Text_RegularExpressions_Regex);

    private static bool IsArgumentConstantOrReadOnly(ObjectCreationExpressionSyntax objectCreationExpression, SemanticModel model) =>
        model.GetConstantValue(objectCreationExpression.ArgumentList?.Arguments[0].Expression).HasValue
        || model.GetSymbolInfo(objectCreationExpression.ArgumentList?.Arguments[0].Expression).Symbol is IFieldSymbol { IsReadOnly: true };

    private static bool IsWithinCorrectContext(SyntaxNode node)
    {
        while (!node.IsKind(SyntaxKind.CompilationUnit))
        {
            if (node.Parent.IsAnyKind(
                    SyntaxKind.MethodDeclaration,
                    SyntaxKind.ConstructorDeclaration,
                    SyntaxKind.DestructorDeclaration,
                    SyntaxKind.GetAccessorDeclaration,
                    SyntaxKind.SetAccessorDeclaration,
                    SyntaxKindEx.LocalFunctionStatement,
                    SyntaxKind.SimpleLambdaExpression,
                    SyntaxKind.ParenthesizedLambdaExpression,
                    SyntaxKind.AnonymousMethodExpression))
            {
                return true;
            }

            node = node.Parent;
        }

        return false;
    }
}
