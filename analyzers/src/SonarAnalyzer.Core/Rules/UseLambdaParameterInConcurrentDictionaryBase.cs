/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

public abstract class UseLambdaParameterInConcurrentDictionaryBase<TSyntaxKind, TInvocationExpression, TArgumentSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocationExpression : SyntaxNode
    where TArgumentSyntax : SyntaxNode
{
    private const string DiagnosticId = "S6612";

    protected override string MessageFormat => "Use the lambda parameter instead of capturing the argument '{0}'";

    protected abstract SeparatedSyntaxList<TArgumentSyntax> GetArguments(TInvocationExpression invocation);
    protected abstract bool TryGetKeyName(TArgumentSyntax argument, out string keyName);
    protected abstract bool IsLambdaAndContainsIdentifier(TArgumentSyntax argument, string keyName);

    protected UseLambdaParameterInConcurrentDictionaryBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var invocation = (TInvocationExpression)c.Node;

            if (HasCorrectName(Language.GetName(invocation))
                && Language.Syntax.Operands(invocation) is { Left: { } left, Right: { } right }
                && IsCorrectType(left, c.Model)
                && IsCorrectCall(right, c.Model)
                && GetArguments(invocation) is { Count: > 1 } arguments
                && TryGetKeyName(arguments[0], out var keyName))
            {
                for (var i = 1; i < arguments.Count; i++)
                {
                    if (IsLambdaAndContainsIdentifier(arguments[i], keyName))
                    {
                        c.ReportIssue(Rule, arguments[i], keyName);
                    }
                }
            }
        }, Language.SyntaxKind.InvocationExpression);

    private bool HasCorrectName(string methodName) =>
        methodName.Equals("GetOrAdd", Language.NameComparison)
        || methodName.Equals("AddOrUpdate", Language.NameComparison);

    private static bool IsCorrectType(SyntaxNode left, SemanticModel model) =>
        model.GetTypeInfo(left).Type.DerivesFrom(KnownType.System_Collections_Concurrent_ConcurrentDictionary_TKey_TValue);

    private static bool IsCorrectCall(SyntaxNode right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && method.ContainingType.Is(KnownType.System_Collections_Concurrent_ConcurrentDictionary_TKey_TValue);
}
