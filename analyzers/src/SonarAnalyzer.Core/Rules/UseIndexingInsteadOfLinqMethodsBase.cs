/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

public abstract class UseIndexingInsteadOfLinqMethodsBase<TSyntaxKind, TInvocation> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocation : SyntaxNode
{
    private const string DiagnosticId = "S6608";
    protected override string MessageFormat => "{0} should be used instead of the \"Enumerable\" extension method \"{1}\"";

    private static readonly ImmutableArray<KnownType> TargetInterfaces = ImmutableArray.Create(
        KnownType.System_Collections_IList,
        KnownType.System_Collections_Generic_IList_T,
        KnownType.System_Collections_Generic_IReadOnlyList_T);

    protected abstract int GetArgumentCount(TInvocation invocation);

    protected UseIndexingInsteadOfLinqMethodsBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var invocation = (TInvocation)c.Node;

            if (HasValidSignature(invocation, out var methodName, out var indexDescriptor)
                && Language.Syntax.Operands(invocation) is { Left: { } left, Right: { } right }
                && IsCorrectType(left, c.Model)
                && IsCorrectCall(right, c.Model))
            {
                c.ReportIssue(
                    Rule,
                    Language.Syntax.NodeIdentifier(invocation)?.GetLocation(),
                    indexDescriptor is null ? "Indexing" : $"Indexing at {indexDescriptor}",
                    methodName);
            }
        },
        Language.SyntaxKind.InvocationExpression);

    private bool HasValidSignature(TInvocation invocation, out string methodName, out string indexDescriptor)
    {
        methodName = Language.GetName(invocation);
        indexDescriptor = null;

        if (methodName.Equals(nameof(Enumerable.First), Language.NameComparison))
        {
            indexDescriptor = "0";
            return GetArgumentCount(invocation) == 0;
        }
        if (methodName.Equals(nameof(Enumerable.Last), Language.NameComparison))
        {
            indexDescriptor = "Count-1";
            return GetArgumentCount(invocation) == 0;
        }
        if (methodName.Equals(nameof(Enumerable.ElementAt), Language.NameComparison))
        {
            return GetArgumentCount(invocation) == 1;
        }

        return false;
    }

    protected static bool IsCorrectType(SyntaxNode left, SemanticModel model) =>
        model.GetTypeInfo(left).Type is { } type
        && (type.ImplementsAny(TargetInterfaces) || type.IsAny(TargetInterfaces));

    protected static bool IsCorrectCall(SyntaxNode right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);
}
