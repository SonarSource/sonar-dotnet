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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public class PublicMethodArgumentsShouldBeCheckedForNull : PublicMethodArgumentsShouldBeCheckedForNullBase
{
    internal static readonly DiagnosticDescriptor S3900 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S3900;

    public override bool ShouldExecute()
    {
        // Should return true if:
        // ✔ method is public, protected or protected internal
        // ✔ method body is not empty
        // ✔ method body is not a single "throw NotImplementedException();"
        // - has parameters with reference type (or nullable value type?)
        // - and at least one of those parameters are dereferenced
        // - parameters don't have attributes which guard against null
        // - nullability is not enabled
        return Node is BaseMethodDeclarationSyntax { } method
         && MethodIsAccesibleFromOtherAssemblies(method)
         && MethodHasBody(method)
         && !MethodOnlyThrowsException(method)
         && MethodBodyContainsDereferencedArguments(method);

        static bool MethodIsAccesibleFromOtherAssemblies(BaseMethodDeclarationSyntax method) =>
            method.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword))
            || (method.Modifiers.Any(x => x.IsKind(SyntaxKind.ProtectedKeyword)) && !method.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword)));

        static bool MethodHasBody(BaseMethodDeclarationSyntax method) =>
            method.Body != null || method.ExpressionBody() != null;

        static bool MethodOnlyThrowsException(BaseMethodDeclarationSyntax method) =>
            ThrowExpressionSyntaxWrapper.IsInstance(method.ExpressionBody()?.Expression)
            || ThrowExpressionSyntaxWrapper.IsInstance(method.Body?.Statements.FirstOrDefault());

        bool MethodBodyContainsDereferencedArguments(BaseMethodDeclarationSyntax method)
        {
            return false;
            //SemanticModel.GetDeclaredSymbol(method).Parameters.Where(x => x.Type.IsNullableValueType() || x.Type.IsReferenceType);
        }
    }
}
