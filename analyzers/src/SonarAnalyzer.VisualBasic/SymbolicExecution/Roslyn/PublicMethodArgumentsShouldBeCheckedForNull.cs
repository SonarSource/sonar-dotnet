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

using static Microsoft.CodeAnalysis.VisualBasic.SyntaxKind;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;

public class PublicMethodArgumentsShouldBeCheckedForNull : PublicMethodArgumentsShouldBeCheckedForNullBase
{
    internal static readonly DiagnosticDescriptor S3900 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S3900;
    protected override string NullName => "Nothing";

    public override bool ShouldExecute()
    {
        return (IsRelevantMethod() || IsRelevantPropertyAccessor())
            && IsAccessibleFromOtherAssemblies();

        bool IsRelevantMethod() =>
            (Node is MethodBlockSyntax { SubOrFunctionStatement: not null } method && MethodDereferencesArguments(method, method.SubOrFunctionStatement.ParameterList))
            || (Node is ConstructorBlockSyntax { SubNewStatement: not null } ctor && MethodDereferencesArguments(ctor, ctor.SubNewStatement.ParameterList));

        bool IsRelevantPropertyAccessor() =>
            Node is AccessorBlockSyntax { } accessor
            && (accessor.Kind() != GetAccessorBlock || accessor.Parent is PropertyBlockSyntax { PropertyStatement.ParameterList: not null });

        static bool MethodDereferencesArguments(SyntaxNode method, ParameterListSyntax parameters)
        {
            var argumentNames = parameters.Parameters.Select(x => x.GetName()).ToHashSet(StringComparer.OrdinalIgnoreCase);
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
        node.Ancestors().OfType<InvocationExpressionSyntax>().Any(x => IsInConstructorInitializer(x.Expression.ToString()));

    private static bool IsInConstructorInitializer(string invokedExpression) =>
        invokedExpression.Equals("Me.New", StringComparison.OrdinalIgnoreCase)
        || invokedExpression.Equals("MyBase.New", StringComparison.OrdinalIgnoreCase);

    private sealed class ArgumentDereferenceWalker : SafeVisualBasicSyntaxWalker
    {
        private readonly ISet<string> argumentNames;

        public bool DereferencesMethodArguments { get; private set; }

        public ArgumentDereferenceWalker(ISet<string> argumentNames) =>
            this.argumentNames = argumentNames;

        public override void Visit(SyntaxNode node)
        {
            if (!DereferencesMethodArguments && !node.IsAnyKind(SingleLineSubLambdaExpression, MultiLineSubLambdaExpression, SingleLineFunctionLambdaExpression, MultiLineFunctionLambdaExpression))
            {
                base.Visit(node);
            }
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node) =>
            DereferencesMethodArguments |=
                argumentNames.Contains(node.GetName())
                && node.HasAncestor(AwaitExpression,
                    InvocationExpression,   // For array access
                    ForEachStatement,
                    ThrowStatement,
                    SimpleMemberAccessExpression);
    }
}
