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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseFind : UseFindBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    private readonly ImmutableArray<KnownType> appliedToTypes =
        ImmutableArray.Create(
            KnownType.System_Collections_Generic_List_T,
            KnownType.System_Array,
            KnownType.System_Collections_Immutable_ImmutableList_T);
    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var invocation = c.Node as InvocationExpressionSyntax;

                if (IsInvocationNamedFirstOrDefault(invocation) && IsInvokedOnAppliedTypes(c, invocation) && IsEnumerableFirstOrDefault(c, invocation))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, invocation.GetIdentifier()?.GetLocation()));
                }
            },
            SyntaxKind.InvocationExpression);

    private static bool IsInvocationNamedFirstOrDefault(InvocationExpressionSyntax invocation) => invocation.NameIs(nameof(Enumerable.FirstOrDefault));

    private bool IsInvokedOnAppliedTypes(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation)
    {
        if (invocation?.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return false;
        }

        var memberTypeSymbol = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;

        return memberTypeSymbol.IsAny(appliedToTypes) || memberTypeSymbol.DerivesFromAny(appliedToTypes);
    }

    private bool IsEnumerableFirstOrDefault(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation) =>
        context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol { Name: nameof(Enumerable.FirstOrDefault) } method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);
}
