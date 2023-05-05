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

public abstract class ExistsInsteadOfAnyBase<TSyntaxKind, TInvocationExpression> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocationExpression : SyntaxNode
{
    private const string DiagnosticId = "S6605";

    private readonly ImmutableArray<KnownType> targetTypes = ImmutableArray.Create(
        KnownType.System_Collections_Generic_List_T,
        KnownType.System_Collections_Immutable_ImmutableList_T);

    protected override string MessageFormat => """Collection-specific "Exists" method should be used instead of the "Any" extension.""";

    protected abstract bool HasAnyArguments(TInvocationExpression node);
    protected abstract bool TryGetOperands(TInvocationExpression node, out SyntaxNode left, out SyntaxNode right);
    protected abstract SyntaxToken? GetIdentifier(TInvocationExpression invocation);

    protected ExistsInsteadOfAnyBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var invocation = c.Node as TInvocationExpression;

            if (Language.GetName(invocation).Equals(nameof(Enumerable.Any), Language.NameComparison)
                && HasAnyArguments(invocation)
                && TryGetOperands(invocation, out var left, out var right)
                && IsCorrectCall(right, c.SemanticModel)
                && IsCorrectType(left, c.SemanticModel))
            {
                c.ReportIssue(Diagnostic.Create(Rule, GetIdentifier(invocation)?.GetLocation()));
            }
        }, Language.SyntaxKind.InvocationExpression);

    private bool IsCorrectType(SyntaxNode left, SemanticModel model) =>
        model.GetTypeInfo(left).Type is { } type && type.DerivesFromAny(targetTypes);

    private bool IsCorrectCall(SyntaxNode right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);
}
