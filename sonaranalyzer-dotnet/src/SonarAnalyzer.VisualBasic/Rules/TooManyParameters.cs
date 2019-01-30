/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class TooManyParameters : TooManyParametersBase<SyntaxKind, ParameterListSyntax>
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } = VisualBasicGeneratedCodeRecognizer.Instance;
        protected override SyntaxKind[] SyntaxKinds { get; } = new SyntaxKind[] { SyntaxKind.ParameterList };

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);

        private static readonly ImmutableDictionary<SyntaxKind, string> nodeToDeclarationName = new Dictionary<SyntaxKind, string>
        {
            { SyntaxKind.SubNewStatement, "Constructor" },
            { SyntaxKind.FunctionStatement, "Function" },
            { SyntaxKind.SubStatement, "Sub" },
            { SyntaxKind.DelegateFunctionStatement, "Delegate" },
            { SyntaxKind.DelegateSubStatement, "Delegate" },
            { SyntaxKind.SubLambdaHeader, "Lambda" },
            { SyntaxKind.FunctionLambdaHeader, "Lambda" },
        }.ToImmutableDictionary();

        private static readonly SyntaxKind[] LambdaHeaders = new[]
        {
            SyntaxKind.FunctionLambdaHeader,
            SyntaxKind.SubLambdaHeader
        };

        protected override string UserFriendlyNameForNode(SyntaxNode node) => nodeToDeclarationName[node.Kind()];

        protected override int CountParameters(ParameterListSyntax parameterList) => parameterList.Parameters.Count;

        protected override bool CanBeChanged(SyntaxNode node, SemanticModel semanticModel)
        {
            if (!nodeToDeclarationName.ContainsKey(node.Kind()))
            {
                return false;
            }

            if ((node as SubNewStatementSyntax)?.ParameterList?.Parameters.Count is int parameterCount &&
                parameterCount > Maximum &&
                node.Parent is ConstructorBlockSyntax constructorBlock &&
                ContainsMyBaseNewInvocation(constructorBlock, Maximum))
            {
                return false;
            }

            if (node.IsAnyKind(LambdaHeaders))
            {
                return true;
            }

            return VerifyCanBeChangedBySymbol(node, semanticModel);
        }

        private static bool ContainsMyBaseNewInvocation(ConstructorBlockSyntax constructorBlock, int maximum) =>
                constructorBlock.Statements.Any(s => s is ExpressionStatementSyntax expression &&
                    expression.Expression is InvocationExpressionSyntax invocation &&
                    invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression is MyBaseExpressionSyntax myBase &&
                    memberAccess.Name.Identifier.Text.Equals("New", System.StringComparison.OrdinalIgnoreCase) &&
                    invocation.ArgumentList.Arguments.Count > maximum);
    }
}
