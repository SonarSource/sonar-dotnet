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

public abstract class UseIndexingInsteadOfLinqMethodsBase<TSyntaxKind, TInvocation> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocation : SyntaxNode
{
    private static readonly ImmutableArray<KnownType> TargetTypes = ImmutableArray.Create(
        KnownType.System_Collections_IList,
        KnownType.System_Collections_Generic_IList_T);

    private const string DiagnosticId = "S6608";
    protected override string MessageFormat => "Indexing {0} should be used instead of the \"Enumerable\" extension method \"{1}\"";

    protected UseIndexingInsteadOfLinqMethodsBase() : base(DiagnosticId) { }

    protected abstract void CheckInvocations(SonarAnalysisContext context);
    protected abstract int GetArgumentCount(TInvocation invocation);
    protected abstract bool TryGetOperands(TInvocation invocation, out SyntaxNode left, out SyntaxNode right);
    protected abstract SyntaxToken? GetIdentifier(TInvocation invocation);

    protected override void Initialize(SonarAnalysisContext context) =>
        CheckInvocations(context);

    protected void CheckInvocation(SonarSyntaxNodeReportingContext c, string methodName, int methodArity, string indexLocation = null)
    {
        var invocation = c.Node as TInvocation;

        if (Language.GetName(invocation).Equals(methodName, Language.NameComparison)
            && GetArgumentCount(invocation) == methodArity
            && TryGetOperands(invocation, out var left, out var right)
            && IsCorrectType(left, c.SemanticModel)
            && IsCorrectCall(right, c.SemanticModel))
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                GetIdentifier(invocation)?.GetLocation(),
                indexLocation == null ? string.Empty : $"at {indexLocation}",
                methodName);
            c.ReportIssue(diagnostic);
        }
    }

    protected static bool IsCorrectType(SyntaxNode left, SemanticModel model) =>
        model.GetTypeInfo(left).Type is { } type && type.ImplementsAny(TargetTypes);

    protected static bool IsCorrectCall(SyntaxNode right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);

}
