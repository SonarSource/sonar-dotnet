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

using SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

namespace SonarAnalyzer.Rules;

public abstract class InsteadOfAnyBase<TSyntaxKind, TInvocationExpression> : SonarDiagnosticAnalyzer
    where TSyntaxKind : struct
    where TInvocationExpression : SyntaxNode
{
    private const string ExistsDiagnosticId = "S6605"; // Collection-specific "Exists" method should be used instead of the "Any" extension.
    private const string ContainsDiagnosticId = "S6617"; // Collection-specific "Contains" method should be used instead of the "Any" extension.
    private const string MessageFormat = "Collection-specific \"{0}\" method should be used instead of the \"Any\" extension.";

    private readonly DiagnosticDescriptor existsRule;
    private readonly DiagnosticDescriptor containsRule;
    private ImmutableArray<KnownType> ExistsTypes { get; } = ImmutableArray.Create(
        KnownType.System_Array,
        KnownType.System_Collections_Immutable_ImmutableList_T);

    private ImmutableArray<KnownType> ContainsTypes { get; } = ImmutableArray.Create(
        KnownType.System_Collections_Generic_HashSet_T,
        KnownType.System_Collections_Generic_SortedSet_T);

    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(existsRule, containsRule);

    protected abstract bool IsSimpleEqualityCheck(TInvocationExpression node, SemanticModel model);
    protected abstract SyntaxNode GetArgumentExpression(TInvocationExpression invocation, int index);
    protected abstract bool AreValidOperands(string lambdaVariable, SyntaxNode first, SyntaxNode second);

    protected InsteadOfAnyBase()
    {
        existsRule = Language.CreateDescriptor(ExistsDiagnosticId, MessageFormat);
        containsRule = Language.CreateDescriptor(ContainsDiagnosticId, MessageFormat);
    }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var invocation = (TInvocationExpression)c.Node;

            if (IsNameEqualTo(invocation, nameof(Enumerable.Any))
                && Language.Syntax.HasExactlyNArguments(invocation, 1)
                && Language.Syntax.TryGetOperands(invocation, out var left, out var right)
                && IsCorrectCall(right, c.SemanticModel)
                && c.SemanticModel.GetTypeInfo(left).Type is { } type)
            {
                if (ExistsTypes.Any(x => type.DerivesFrom(x)))
                {
                    RaiseExists(c, invocation);
                }
                else if (ContainsTypes.Any(x => type.DerivesFrom(x) && IsSimpleEqualityCheck(invocation, c.SemanticModel)))
                {
                    RaiseContains(c, invocation);
                }
                else if (type.DerivesFrom(KnownType.System_Collections_Generic_List_T))
                {
                    if (IsSimpleEqualityCheck(invocation, c.SemanticModel))
                    {
                        RaiseContains(c, invocation);
                    }
                    else
                    {
                        RaiseExists(c, invocation);
                    }
                }
            }
        }, Language.SyntaxKind.InvocationExpression);

    protected bool IsNameEqualTo(SyntaxNode node, string name) =>
        Language.GetName(node).Equals(name, Language.NameComparison);

    protected static bool IsValueTypeOrString(SyntaxNode expression, SemanticModel model) =>
        model.GetTypeInfo(expression).Type is { } type
        && (type.IsValueType || type.Is(KnownType.System_String));

    protected bool IsSimpleEqualsInvocation(TInvocationExpression invocation, string lambdaVariableName)
    {
        if (IsNameEqualTo(invocation, nameof(Equals)))
        {
            if (Language.Syntax.HasExactlyNArguments(invocation, 1))
            {
                return Language.Syntax.TryGetOperands(invocation, out var left, out _)
                    && HasInvocationValidOperands(left, GetArgumentExpression(invocation, 0));
            }
            if (Language.Syntax.HasExactlyNArguments(invocation, 2))
            {
                return HasInvocationValidOperands(GetArgumentExpression(invocation, 0), GetArgumentExpression(invocation, 1));
            }
        }
        return false;

        bool HasInvocationValidOperands(SyntaxNode first, SyntaxNode second) =>
            AreValidOperands(lambdaVariableName, first, second) || AreValidOperands(lambdaVariableName, second, first);
    }

    private static bool IsCorrectCall(SyntaxNode right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);

    private void RaiseExists(SonarSyntaxNodeReportingContext c, SyntaxNode invocation) =>
        c.ReportIssue(Diagnostic.Create(existsRule, Language.Syntax.NodeIdentifier(invocation)?.GetLocation(), "Exists"));

    private void RaiseContains(SonarSyntaxNodeReportingContext c, SyntaxNode invocation) =>
        c.ReportIssue(Diagnostic.Create(containsRule, Language.Syntax.NodeIdentifier(invocation)?.GetLocation(), "Contains"));
}
