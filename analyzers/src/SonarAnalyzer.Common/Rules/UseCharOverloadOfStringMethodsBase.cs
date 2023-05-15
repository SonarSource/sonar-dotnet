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

public abstract class UseCharOverloadOfStringMethodsBase<TSyntaxKind, TInvocation> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocation : SyntaxNode
{
    private const string DiagnosticId = "S6610";
    protected override string MessageFormat => "\"{0}\" overloads that take a \"char\" should be used";

    // FIXME: Remove this
    protected override bool EnableConcurrentExecution => false;

    protected abstract bool HasCorrectArguments(TInvocation invocation);

    protected UseCharOverloadOfStringMethodsBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var invocation = (TInvocation)c.Node;
            var methodName = Language.GetName(invocation);
            if (HasCorrectName(methodName)
                && HasCorrectArguments(invocation)
                && Language.Syntax.TryGetOperands(invocation, out var left, out _)
                //&& CompilationRunsOnValidNetVersion(c.Compilation)
                && IsCorrectType(left, c.SemanticModel))
            {
                c.ReportIssue(Diagnostic.Create(Rule, Language.Syntax.NodeIdentifier(invocation)?.GetLocation(), methodName));
            }
        }, Language.SyntaxKind.InvocationExpression);

    // "char" overload introduced at .NET Core 2.0
    private bool CompilationRunsOnValidNetVersion(Compilation compilation) => throw new NotImplementedException();

    private bool HasCorrectName(string methodName) =>
        methodName.Equals(nameof(string.StartsWith), Language.NameComparison)
        || methodName.Equals(nameof(string.EndsWith), Language.NameComparison);

    private static bool IsCorrectType(SyntaxNode left, SemanticModel model) =>
        model.GetTypeInfo(left).Type is { } type && type.Is(KnownType.System_String);
}
