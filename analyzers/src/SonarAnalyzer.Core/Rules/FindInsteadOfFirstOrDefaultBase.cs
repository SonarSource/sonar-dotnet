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

public abstract class FindInsteadOfFirstOrDefaultBase<TSyntaxKind, TInvocationExpression> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocationExpression : SyntaxNode
{
    private const string DiagnosticId = "S6602";
    private const int NumberOfArgument = 1;
    private const string GenericMessage = @"""Find"" method should be used instead of the ""FirstOrDefault"" extension method.";
    private const string ArrayMessage = @"""Array.Find"" static method should be used instead of the ""FirstOrDefault"" extension method.";

    protected override string MessageFormat => "{0}";

    private static readonly ImmutableArray<KnownType> ListTypes = ImmutableArray.Create(
        KnownType.System_Collections_Generic_List_T,
        KnownType.System_Collections_Immutable_ImmutableList_T);

    protected FindInsteadOfFirstOrDefaultBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
            {
                var invocation = (TInvocationExpression)c.Node;

                if (Language.GetName(invocation).Equals(nameof(Enumerable.FirstOrDefault), Language.NameComparison)
                    && Language.Syntax.HasExactlyNArguments(invocation, NumberOfArgument)
                    && Language.Syntax.TryGetOperands(invocation, out var left, out var right)
                    && IsCorrectCall(right, c.Model)
                    && IsCorrectType(left, c.Model, out var isArray)
                    && !Language.Syntax.IsInExpressionTree(c.Model, invocation))
                {
                    c.ReportIssue(Rule, Language.Syntax.NodeIdentifier(invocation)?.GetLocation(), isArray ? ArrayMessage : GenericMessage);
                }
            },
            Language.SyntaxKind.InvocationExpression);

    private static bool IsCorrectCall(SyntaxNode right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
        && method.Parameters.Length == NumberOfArgument
        && method.Parameters[0].IsType(KnownType.System_Func_T_TResult);

    private static bool IsCorrectType(SyntaxNode left, SemanticModel model, out bool isArray)
    {
        var type = model.GetTypeInfo(left).Type;
        isArray = type.DerivesFrom(KnownType.System_Array);
        return isArray || type.DerivesFromAny(ListTypes);
    }
}
