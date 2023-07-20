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
    internal const string DiagnosticId = "S6610";

    protected override string MessageFormat => "\"{0}\" overloads that take a \"char\" should be used";

    protected abstract bool HasCorrectArguments(TInvocation invocation);

    protected UseCharOverloadOfStringMethodsBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(start =>
        {
            if (!CompilationTargetsValidNetVersion(start.Compilation))
            {
                return;
            }

            start.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
            {
                var invocation = (TInvocation)c.Node;
                var methodName = Language.GetName(invocation);

                if (HasCorrectName(methodName)
                    && HasCorrectArguments(invocation)
                    && Language.Syntax.TryGetOperands(invocation, out var left, out _)
                    && IsCorrectType(left, c.SemanticModel))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, Language.Syntax.NodeIdentifier(invocation)?.GetLocation(), methodName));
                }
            }, Language.SyntaxKind.InvocationExpression);
        });

    // "char" overload introduced at .NET Core 2.0/.NET Standard 2.1
    private static bool CompilationTargetsValidNetVersion(Compilation compilation)
    {
        var stringType = compilation.GetTypeByMetadataName(KnownType.System_String);
        var methods = stringType.GetMembers(nameof(string.StartsWith)).Where(x => x is IMethodSymbol);
        return methods.Any(x => ((IMethodSymbol)x).Parameters[0].IsType(KnownType.System_Char));
    }

    private bool HasCorrectName(string methodName) =>
        methodName.Equals(nameof(string.StartsWith), Language.NameComparison)
        || methodName.Equals(nameof(string.EndsWith), Language.NameComparison);

    private static bool IsCorrectType(SyntaxNode left, SemanticModel model) =>
        model.GetTypeInfo(left).Type.Is(KnownType.System_String);
}
