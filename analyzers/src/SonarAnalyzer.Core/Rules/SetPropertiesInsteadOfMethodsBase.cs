/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules;

public abstract class SetPropertiesInsteadOfMethodsBase<TSyntaxKind, TInvocation> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocation : SyntaxNode
{
    private const string DiagnosticId = "S6609";
    protected override string MessageFormat => "\"{0}\" property of Set type should be used instead of the \"{0}()\" extension method.";

    private static readonly ImmutableArray<KnownType> TargetTypes = ImmutableArray.Create(
        KnownType.System_Collections_Generic_SortedSet_T,
        KnownType.System_Collections_Immutable_ImmutableSortedSet_T);

    protected abstract bool HasCorrectArgumentCount(TInvocation invocation);
    protected abstract bool TryGetOperands(TInvocation invocation, out SyntaxNode typeNode, out SyntaxNode methodNode);

    protected SetPropertiesInsteadOfMethodsBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var invocation = (TInvocation)c.Node;
            var methodName = Language.GetName(invocation);

            if (HasCorrectName(methodName)
                && HasCorrectArgumentCount(invocation)
                && TryGetOperands(invocation, out var typeNode, out var methodNode)
                && IsCorrectType(typeNode, c.Model)
                && IsCorrectCall(methodNode, c.Model))
            {
                c.ReportIssue(Rule, Language.Syntax.NodeIdentifier(invocation)?.GetLocation(), methodName);
            }
        }, Language.SyntaxKind.InvocationExpression);

    private bool HasCorrectName(string methodName) =>
        methodName.Equals(nameof(Enumerable.Min), Language.NameComparison)
        || methodName.Equals(nameof(Enumerable.Max), Language.NameComparison);

    private static bool IsCorrectType(SyntaxNode node, SemanticModel model) =>
        model.GetTypeInfo(node).Type.DerivesFromAny(TargetTypes);

    private static bool IsCorrectCall(SyntaxNode node, SemanticModel model) =>
        model.GetSymbolInfo(node).Symbol is IMethodSymbol method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);
}
