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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Rules;

public abstract class UseFindBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6602";

    private readonly ImmutableArray<KnownType> appliedToTypes =
        ImmutableArray.Create(
            KnownType.System_Collections_Generic_List_T,
            KnownType.System_Array,
            KnownType.System_Collections_Immutable_ImmutableList_T);

    protected override string MessageFormat => $"\"{nameof(Array.Find)}\" method should be used instead of the \"{nameof(Enumerable.FirstOrDefault)}\" extension method.";

    protected UseFindBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
            {
                var (syntaxNode, leftExpression, rightExpression) = c.Node switch
                {
                    ConditionalAccessExpressionSyntax { WhenNotNull: InvocationExpressionSyntax inv } condAccess => ((SyntaxNode)inv, condAccess.Expression, inv.Expression),
                    MemberAccessExpressionSyntax memberAccess => (memberAccess, memberAccess.Expression, memberAccess.Name),
                    _ => (null, null, null)
                };

                if (syntaxNode is not null && IsInvocationNamedFirstOrDefault(syntaxNode) && IsInvokedOnAppliedTypes(c, leftExpression) && IsEnumerableFirstOrDefault(c, rightExpression))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, GetIssueLocation(syntaxNode)));
                }
            },
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxKind.ConditionalAccessExpression);

    protected abstract bool IsInvocationNamedFirstOrDefault(SyntaxNode syntaxNode);
    protected abstract Location GetIssueLocation(SyntaxNode syntaxNode);

    private bool IsInvokedOnAppliedTypes(SonarSyntaxNodeReportingContext context, ExpressionSyntax expression)
    {
        var memberTypeSymbol = context.SemanticModel.GetTypeInfo(expression).Type;

        return memberTypeSymbol.IsAny(appliedToTypes) || memberTypeSymbol.DerivesFromAny(appliedToTypes);
    }

    private bool IsEnumerableFirstOrDefault(SonarSyntaxNodeReportingContext context, ExpressionSyntax expression) =>
        context.SemanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol { Name: nameof(Enumerable.FirstOrDefault) } method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);
}
