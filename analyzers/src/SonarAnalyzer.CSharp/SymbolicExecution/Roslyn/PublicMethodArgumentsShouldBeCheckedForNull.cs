/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.Common.Walkers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using static StyleCop.Analyzers.Lightup.SyntaxKindEx;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public class PublicMethodArgumentsShouldBeCheckedForNull : PublicMethodArgumentsShouldBeCheckedForNullBase
{
    internal static readonly DiagnosticDescriptor S3900 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S3900;
    protected override string NullName => "null";

    public override bool ShouldExecute()
    {
        return (IsRelevantMethod() || IsRelevantPropertyAccessor())
            && IsAccessibleFromOtherAssemblies();

        bool IsRelevantMethod() =>
            Node is BaseMethodDeclarationSyntax { } method && MethodDereferencesArguments(method);

        bool IsRelevantPropertyAccessor() =>
            Node is AccessorDeclarationSyntax { } accessor
            && (!accessor.Keyword.IsKind(GetKeyword) || accessor.Parent.Parent is IndexerDeclarationSyntax);

        static bool MethodDereferencesArguments(BaseMethodDeclarationSyntax method)
        {
            var argumentNames = method.ParameterList.Parameters
                                    .Where(x => !x.Modifiers.AnyOfKind(OutKeyword))
                                    .Select(x => x.GetName())
                                    .ToHashSet();
            if (argumentNames.Any())
            {
                var walker = new ArgumentDereferenceWalker(argumentNames);
                walker.SafeVisit(method);
                return walker.DereferencesMethodArguments;
            }
            else
            {
                return false;
            }
        }
    }

    protected override bool IsInConstructorInitializer(SyntaxNode node) =>
        node.FirstAncestorOrSelf<ConstructorInitializerSyntax>() is not null;

    private sealed class ArgumentDereferenceWalker : SafeCSharpSyntaxWalker
    {
        private readonly ISet<string> argumentNames;

        public bool DereferencesMethodArguments { get; private set; }

        public ArgumentDereferenceWalker(ISet<string> argumentNames) =>
            this.argumentNames = argumentNames;

        public override void Visit(SyntaxNode node)
        {
            if (!DereferencesMethodArguments && !node.IsAnyKind(LocalFunctionStatement, SimpleLambdaExpression, ParenthesizedLambdaExpression))
            {
                base.Visit(node);
            }
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node) =>
            DereferencesMethodArguments |=
                argumentNames.Contains(node.GetName())
                && node.HasAncestor(AwaitExpression,
                    ElementAccessExpression,
                    ForEachStatement,
                    ThrowStatement,
                    SimpleMemberAccessExpression);
    }
}
