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

public abstract class FindInsteadOfFirstOrDefaultBase<TSyntaxKind, TInvocationExpression> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocationExpression : SyntaxNode
{
    private const string DiagnosticId = "S6602";
    private const int NumberOfArgument = 1;

    protected override string MessageFormat => $"\"{nameof(Array.Find)}\" method should be used instead of the \"{nameof(Enumerable.FirstOrDefault)}\" extension method.";

    private static readonly ImmutableArray<KnownType> RuleSpecificTypes = ImmutableArray.Create(
        KnownType.System_Collections_Generic_List_T,
        KnownType.System_Array,
        KnownType.System_Collections_Immutable_ImmutableList_T);

    protected FindInsteadOfFirstOrDefaultBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
            {
                var invocation = (TInvocationExpression)c.Node;

                if (Language.GetName(invocation).Equals(nameof(Enumerable.FirstOrDefault), Language.NameComparison)
                    && Language.Syntax.HasExactlyNArguments(invocation, NumberOfArgument)
                    && Language.Syntax.TryGetOperands(invocation, out var left, out var right)
                    && IsCorrectCall(right, c.SemanticModel)
                    && IsCorrectType(left, c.SemanticModel))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, Language.Syntax.NodeIdentifier(invocation)?.GetLocation()));
                }
            },
            Language.SyntaxKind.InvocationExpression);

    private static bool IsCorrectCall(SyntaxNode right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
        && method.Parameters.Length == NumberOfArgument
        && method.Parameters[0].IsType(KnownType.System_Func_T_TResult);

    private static bool IsCorrectType(SyntaxNode left, SemanticModel model) =>
        model.GetTypeInfo(left).Type.DerivesFromAny(RuleSpecificTypes);
}
