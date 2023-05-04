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

namespace SonarAnalyzer.Rules.CSharp;

public class UseTrueForAll : UseThisInsteadOfThat
{
    protected override string GetMethodName => "ToString";
    protected override bool MethodCondition(IMethodSymbol method) => method.Is(KnownType.System_Text_StringBuilder, "ToString");
    protected override bool TypeCondition(ITypeSymbol type) => type.DerivesFrom(KnownType.System_Text_StringBuilder);
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public abstract class UseThisInsteadOfThat : UseMethodAInsteadOfMethodB<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override bool EnableConcurrentExecution => false;

    protected abstract string GetMethodName { get; }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
        {
            var invocation = c.Node as InvocationExpressionSyntax;

            if (invocation.NameIs(GetMethodName)
                && TryGetOperands(invocation, out var left, out var right)
                && IsCorrectCall(right, c.SemanticModel)
                && IsCorrectType(left, c.SemanticModel))
            {
                c.ReportIssue(Diagnostic.Create(Rule, c.Node.GetLocation()));
            }
        },
        SyntaxKind.InvocationExpression);

    protected static bool TryGetOperands(InvocationExpressionSyntax invocation, out ExpressionSyntax left, out ExpressionSyntax right)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax access)
        {
            left = access.Expression;
            right = access.Name;
            return true;
        }
        else if (invocation.Expression is MemberBindingExpressionSyntax binding)
        {
            left = GetLeft(invocation);
            right = binding.Name;
            return true;
        }

        left = right = null;
        return false;
    }

    protected static ExpressionSyntax GetLeft(SyntaxNode current, int iteration = 0)
    {
        const int magicNumber = 42;
        if (iteration > magicNumber || current.Parent is CompilationUnitSyntax)
        {
            return null;
        }

        if (current.Parent is ConditionalAccessExpressionSyntax conditional && conditional.WhenNotNull == current)
        {
            return conditional.Expression;
        }

        return GetLeft(current.Parent, iteration + 1);
    }

    private bool IsCorrectType(ExpressionSyntax left, SemanticModel model) =>
        model.GetTypeInfo(left).Type is { } type
        && TypeCondition(type);

    private bool IsCorrectCall(ExpressionSyntax right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && MethodCondition(method);

    protected abstract bool TypeCondition(ITypeSymbol type);
    //    type.Is(KnownType.System_Collections_Generic_List_T); // T is TargetT
    //    type.DerivesFrom(KnownType.System_Collections_Generic_List_T); // T is/derives TargetT
    //    type.Implements(KnownType.System_Collections_Generic_IEnumerable_T);  // T implements TargetInterface

    protected abstract bool MethodCondition(IMethodSymbol method);
    // method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T); // extension method on IEnumerable, e.g. All(this IEnumerable...)
    // method.Is(KnownType.System_Collections_Generic_HashSet_T, "MethodName"); // method is _exactly_ T.Method(..)
}
