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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public class PublicMethodArgumentsShouldBeCheckedForNull : SymbolicRuleCheck
{
    private const string DiagnosticId = "S3900";
    private const string MessageFormat = "{0}";

    internal static readonly DiagnosticDescriptor S3900 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S3900;

    public override bool ShouldExecute()
    {
        return IsAccessibleFromOtherAssemblies(Node)
            && (IsRelevantMethod(Node) || IsRelevantPropertyAccessor(Node));

        static bool IsRelevantMethod(SyntaxNode node) =>
            node is BaseMethodDeclarationSyntax { } method
            && MethodDereferencesArguments(method);

        static bool IsRelevantPropertyAccessor(SyntaxNode node) =>
            node is AccessorDeclarationSyntax accessor
            && IsPropertyAccessorAccessibleFromOtherAssemblies(accessor.Modifiers);

        static bool IsAccessibleFromOtherAssemblies(SyntaxNode node) =>
            node.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().FirstOrDefault() is { } containingMember
            && node.Ancestors().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault() is { } containingType
            && IsMemberAccessibleFromOtherAssemblies(containingMember.Modifiers(), containingType)
            && IsTypeAccessibleFromOtherAssemblies(containingType.Modifiers);

        static bool IsMemberAccessibleFromOtherAssemblies(SyntaxTokenList modifiers, BaseTypeDeclarationSyntax containingType) =>
            IsPublicOrProtectedOrProtectedInternal(modifiers)
            || (containingType is InterfaceDeclarationSyntax && HasNoDeclaredAccessibilityModifier(modifiers));

        static bool IsPropertyAccessorAccessibleFromOtherAssemblies(SyntaxTokenList modifiers) =>
            IsPublicOrProtectedOrProtectedInternal(modifiers)
            || HasNoDeclaredAccessibilityModifier(modifiers);

        static bool IsTypeAccessibleFromOtherAssemblies(SyntaxTokenList modifiers) =>
            IsPublicOrProtectedOrProtectedInternal(modifiers);

        static bool IsPublicOrProtectedOrProtectedInternal(SyntaxTokenList modifiers) =>
            modifiers.AnyOfKind(SyntaxKind.PublicKeyword)
            || (modifiers.AnyOfKind(SyntaxKind.ProtectedKeyword) && !modifiers.AnyOfKind(SyntaxKind.PrivateKeyword));

        static bool HasNoDeclaredAccessibilityModifier(SyntaxTokenList modifiers) =>
            !modifiers.Any(x => x.IsAnyKind(
                SyntaxKind.PrivateKeyword,
                SyntaxKind.ProtectedKeyword,
                SyntaxKind.InternalKeyword,
                SyntaxKind.PublicKeyword));

        static bool MethodDereferencesArguments(BaseMethodDeclarationSyntax method)
        {
            var argumentNames = method.ParameterList.Parameters
                                    .Where(x => !x.Modifiers.AnyOfKind(SyntaxKind.OutKeyword))
                                    .Select(x => x.GetName())
                                    .ToArray();

            if (!argumentNames.Any())
            {
                return false;
            }

            var walker = new ArgumentDereferenceWalker(argumentNames);
            walker.SafeVisit(method);
            return walker.DereferencesMethodArguments;
        }
    }

    private sealed class ArgumentDereferenceWalker : SafeCSharpSyntaxWalker
    {
        private readonly string[] argumentNames;

        public bool DereferencesMethodArguments { get; private set; }

        public ArgumentDereferenceWalker(string[] argumentNames) =>
            this.argumentNames = argumentNames;

        public override void Visit(SyntaxNode node)
        {
            if (!DereferencesMethodArguments
             && !IsStaticLocalFunction(node)
             && !IsStaticLambda(node))
            {
                base.Visit(node);
            }

            static bool IsStaticLocalFunction(SyntaxNode node) =>
                LocalFunctionStatementSyntaxWrapper.IsInstance(node)
                && ((LocalFunctionStatementSyntaxWrapper)node).Modifiers.AnyOfKind(SyntaxKind.StaticKeyword);

            static bool IsStaticLambda(SyntaxNode node) =>
                SimpleLambdaExpressionSyntaxWrapper.IsInstance(node)
                && ((SimpleLambdaExpressionSyntaxWrapper)node).Modifiers.AnyOfKind(SyntaxKind.StaticKeyword);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node) =>
            DereferencesMethodArguments |=
                argumentNames.Contains(node.GetName())
                && node.Parent.IsAnyKind(
                    SyntaxKind.AwaitExpression,
                    SyntaxKind.ElementAccessExpression,
                    SyntaxKind.ForEachStatement,
                    SyntaxKind.ThrowStatement,
                    SyntaxKind.SimpleMemberAccessExpression);
    }
}
