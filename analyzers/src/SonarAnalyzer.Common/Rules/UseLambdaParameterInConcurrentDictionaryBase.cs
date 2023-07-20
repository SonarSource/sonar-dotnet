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
                && Language.Syntax.TryGetOperands(invocation, out var left, out var right)
                && IsCorrectType(left, c.SemanticModel)
                && IsCorrectCall(right, c.SemanticModel)
                && GetArguments(invocation) is { Count: > 1 } arguments
                && TryGetKeyName(arguments[0], out var keyName))
            {
                for (var i = 1; i < arguments.Count; i++)
                {
                    if (IsLambdaAndContainsIdentifier(arguments[i], keyName))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, arguments[i].GetLocation(), keyName));
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
