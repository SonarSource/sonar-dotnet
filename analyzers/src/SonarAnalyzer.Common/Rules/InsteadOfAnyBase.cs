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

namespace SonarAnalyzer.Rules;

public abstract class InsteadOfAnyBase<TSyntaxKind, TInvocationExpression> : SonarDiagnosticAnalyzer
    where TSyntaxKind : struct
    where TInvocationExpression : SyntaxNode
{
    private const string ExistsDiagnosticId = "S6605";
    private const string ContainsDiagnosticId = "S6617";
    private const string MessageFormat = "Collection-specific \"{0}\" method should be used instead of the \"Any\" extension.";

    private readonly DiagnosticDescriptor existsRule;
    private readonly DiagnosticDescriptor containsRule;

    private static readonly ImmutableArray<KnownType> ExistsTypes = ImmutableArray.Create(
        KnownType.System_Array,
        KnownType.System_Collections_Immutable_ImmutableList_T);

    private static readonly ImmutableArray<KnownType> ContainsTypes = ImmutableArray.Create(
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
                && c.SemanticModel.GetTypeInfo(left).Type is { } type
                && !Language.Syntax.IsInExpressionTree(c.SemanticModel, invocation))
            {
                if (ExistsTypes.Any(x => type.DerivesFrom(x)))
                {
                    RaiseIssue(c, invocation, existsRule, "Exists");
                }
                else if (ContainsTypes.Any(x => type.DerivesFrom(x) && IsSimpleEqualityCheck(invocation, c.SemanticModel)))
                {
                    RaiseIssue(c, invocation, containsRule, "Contains");
                }
                else if (type.DerivesFrom(KnownType.System_Collections_Generic_List_T))
                {
                    if (IsSimpleEqualityCheck(invocation, c.SemanticModel))
                    {
                        RaiseIssue(c, invocation, containsRule, "Contains");
                    }
                    else
                    {
                        RaiseIssue(c, invocation, existsRule, "Exists");
                    }
                }
            }
        }, Language.SyntaxKind.InvocationExpression);

    protected bool IsNameEqualTo(SyntaxNode node, string name) =>
        Language.GetName(node).Equals(name, Language.NameComparison);

    protected static bool IsValueTypeOrString(SyntaxNode expression, SemanticModel model) =>
        model.GetTypeInfo(expression).Type is { } type
        && (type.IsValueType || type.Is(KnownType.System_String));

    protected bool HasValidInvocationOperands(TInvocationExpression invocation, string lambdaVariableName, SemanticModel model)
    {
        if (IsNameEqualTo(invocation, nameof(Equals)))
        {
            if (Language.Syntax.HasExactlyNArguments(invocation, 1)) // x.Equals(y)
            {
                return Language.Syntax.TryGetOperands(invocation, out var left, out _)
                    && HasInvocationValidOperands(left, GetArgumentExpression(invocation, 0))
                    && IsSystemEquals();
            }
            if (Language.Syntax.HasExactlyNArguments(invocation, 2)) // Equals(x,y)
            {
                return HasInvocationValidOperands(GetArgumentExpression(invocation, 0), GetArgumentExpression(invocation, 1))
                    && IsSystemEquals();
            }
        }
        return false;

        bool HasInvocationValidOperands(SyntaxNode first, SyntaxNode second) =>
            AreValidOperands(lambdaVariableName, first, second) || AreValidOperands(lambdaVariableName, second, first);

        bool IsSystemEquals() =>
            model.GetSymbolInfo(invocation).Symbol.ContainingNamespace.Name == nameof(System);
    }

    private static bool IsCorrectCall(SyntaxNode right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);

    private void RaiseIssue(SonarSyntaxNodeReportingContext c, SyntaxNode invocation, DiagnosticDescriptor rule, string methodName) =>
        c.ReportIssue(CreateDiagnostic(rule, Language.Syntax.NodeIdentifier(invocation)?.GetLocation(), methodName));
}
