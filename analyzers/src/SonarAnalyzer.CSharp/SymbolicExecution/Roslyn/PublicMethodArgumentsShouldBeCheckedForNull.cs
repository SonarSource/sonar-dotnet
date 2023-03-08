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
        return Node is BaseMethodDeclarationSyntax { } method
            && MethodIsAccesibleFromOtherAssemblies(method)
            && MethodHasBody(method)
            && !MethodOnlyThrowsException(method)
            && MethodBodyDereferencesArguments(method);

        static bool MethodIsAccesibleFromOtherAssemblies(BaseMethodDeclarationSyntax method) =>
            method.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword))
            || (method.Modifiers.Any(x => x.IsKind(SyntaxKind.ProtectedKeyword)) && !method.Modifiers.Any(x => x.IsKind(SyntaxKind.PrivateKeyword)));

        static bool MethodHasBody(BaseMethodDeclarationSyntax method) =>
            (method.Body != null && method.Body.Statements.Any()) || method.ExpressionBody() != null;

        static bool MethodOnlyThrowsException(BaseMethodDeclarationSyntax method) =>
            ThrowExpressionSyntaxWrapper.IsInstance(method.ExpressionBody()?.Expression)
            || ThrowExpressionSyntaxWrapper.IsInstance(method.Body?.Statements.FirstOrDefault());

        static bool MethodBodyDereferencesArguments(BaseMethodDeclarationSyntax method)
        {
            var argumentNames = method.ParameterList.Parameters.Select(x => x.Identifier.ValueText).ToArray();
            if (!argumentNames.Any())
            {
                return false;
            }

            var walker = new ArgumentDereferenceWalker(argumentNames);
            return walker.PossiblyDereferencesMethodArguments;
        }
    }

    private sealed class ArgumentDereferenceWalker : SafeCSharpSyntaxWalker
    {
        private readonly string[] argumentNames;

        public bool PossiblyDereferencesMethodArguments { get; private set; }

        public ArgumentDereferenceWalker(string[] argumentNames) =>
            this.argumentNames = argumentNames;

        public override void Visit(SyntaxNode node)
        {
            if (PossiblyDereferencesMethodArguments)
            {
                return;
            }

            if (node is IdentifierNameSyntax { } identifier
                && argumentNames.Contains(identifier.Identifier.ValueText)
                && identifier.Ancestors().Any(x => x.IsAnyKind(
                    SyntaxKind.AwaitExpression,
                    SyntaxKind.ElementAccessExpression,
                    SyntaxKind.ForEachStatement,
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxKind.VariableDeclarator)))
            {
                PossiblyDereferencesMethodArguments = true;
            }
        }
    }
}
