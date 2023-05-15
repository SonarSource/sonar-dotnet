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

public abstract class InsteadOfAny<TSyntaxKind, TInvocationExpression> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocationExpression : SyntaxNode
{
    protected abstract ImmutableArray<(KnownType Type, bool CheckContext)> RuleSpecificTypes { get; }
    protected abstract bool IsSimpleEqualityCheck(TInvocationExpression node, SemanticModel model);
    protected abstract bool HasOneArgument(TInvocationExpression node);
    protected abstract bool IsInValidContext(TInvocationExpression invocation, SemanticModel model);

    protected InsteadOfAny(string diagnosticId) : base(diagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var invocation = (TInvocationExpression)c.Node;

            if (IsNameEqual(invocation, nameof(Enumerable.Any))
                && HasOneArgument(invocation)
                && Language.Syntax.TryGetOperands(invocation, out var left, out var right)
                && IsCorrectCall(right, c.SemanticModel)
                && c.SemanticModel.GetTypeInfo(left).Type is { } type
                && IsCorrectTypeAndContext(type, invocation, c.SemanticModel))
            {
                c.ReportIssue(Diagnostic.Create(Rule, Language.Syntax.NodeIdentifier(invocation)?.GetLocation()));
            }
        }, Language.SyntaxKind.InvocationExpression);

    protected bool IsNameEqual(SyntaxNode node, string name) =>
        Language.GetName(node).Equals(name, Language.NameComparison);

    protected static bool IsValueTypeOrString(SyntaxNode expression, SemanticModel model) =>
        model.GetTypeInfo(expression).Type is { } type
        && (type.IsValueType || type.Is(KnownType.System_String));

    protected bool IsCorrectTypeAndContext(ITypeSymbol type, TInvocationExpression invocation, SemanticModel model) =>
        RuleSpecificTypes.Any(x => type.DerivesFrom(x.Type) && (!x.CheckContext || IsInValidContext(invocation, model)));

    protected static bool IsCorrectCall(SyntaxNode right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);
}
