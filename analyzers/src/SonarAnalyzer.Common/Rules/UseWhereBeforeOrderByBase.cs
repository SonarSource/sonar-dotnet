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

public abstract class UseWhereBeforeOrderByBase<TSyntaxKind, TInvocation> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocation : SyntaxNode
{
    private const string DiagnosticId = "S6607";
    protected override string MessageFormat => "\"Where\" should be used before \"{0}\"";

    protected UseWhereBeforeOrderByBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            var invocation = (TInvocation)c.Node;

            if (Language.GetName(invocation).Equals("Where", Language.NameComparison)
                && Language.Syntax.TryGetOperands(invocation, out var left, out var right)
                && LeftHasCorrectName(left, out var orderByMethodDescription)
                && MethodIsLinqExtension(left, c.SemanticModel)
                && MethodIsLinqExtension(right, c.SemanticModel))
            {
                var diagnostic = CreateDiagnostic(
                    Rule,
                    Language.Syntax.NodeIdentifier(right)?.GetLocation(),
                    new[] { Language.Syntax.NodeIdentifier(left)?.GetLocation() },
                    orderByMethodDescription);
                c.ReportIssue(diagnostic);
            }
        },
        Language.SyntaxKind.InvocationExpression);

    private bool LeftHasCorrectName(SyntaxNode left, out string methodName)
    {
        var leftName = Language.GetName(left);
        if (leftName.Equals("OrderBy", Language.NameComparison)
            || leftName.Equals("OrderByDescending", Language.NameComparison))
        {
            methodName = leftName;
            return true;
        }
        methodName = null;
        return false;
    }

    private static bool MethodIsLinqExtension(SyntaxNode node, SemanticModel model) =>
        model.GetSymbolInfo(node).Symbol is IMethodSymbol method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);
}
