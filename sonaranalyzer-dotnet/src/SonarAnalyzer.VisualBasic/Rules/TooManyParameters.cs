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
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => VisualBasicGeneratedCodeRecognizer.Instance;
        protected override SyntaxKind[] SyntaxKinds => new SyntaxKind[] { SyntaxKind.ParameterList };
        protected override DiagnosticDescriptor Rule => rule;
        protected override Dictionary<SyntaxKind, string> Mapping => mapping;
        protected override SyntaxKind ParentType(ParameterListSyntax parameterList) => parameterList.Parent.Kind();
        protected override int CountParameters(ParameterListSyntax parameterList) => parameterList.Parameters.Count;
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override bool CanBeChanged(SyntaxNode node, SemanticModel semanticModel)
        {
            var declaredSymbol = semanticModel.GetDeclaredSymbol(node);
            var symbol = semanticModel.GetSymbolInfo(node).Symbol;

            if (node.IsAnyKind(LambdaHeaders))
            {
                return true;
            }

            if (declaredSymbol == null && symbol == null)
            {
                // no information
                return false;
            }

            if (symbol != null)
            {
                // Not a declaration, such as Action
                return true;
            }

            if ((node as SubNewStatementSyntax)?.ParameterList?.Parameters.Count is int parameterCount &&
                parameterCount > Maximum &&
                node.Parent is ConstructorBlockSyntax constructorBlock &&
                ContainsMyBaseNewInvocation(constructorBlock, Maximum))
            {
                return false;
            }

            if (declaredSymbol.IsExtern &&
                declaredSymbol.IsStatic &&
                declaredSymbol.GetAttributes(KnownType.System_Runtime_InteropServices_DllImportAttribute).Any())
            {
                // P/Invoke method is defined externally.
                // Do not raise
                return false;
            }

            return declaredSymbol.GetOverriddenMember() == null &&
                   declaredSymbol.GetInterfaceMember() == null;
        }

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);

        private static bool ContainsMyBaseNewInvocation(ConstructorBlockSyntax constructorBlock, int maximum) =>
                constructorBlock.Statements.Any(s => s is ExpressionStatementSyntax expression &&
                    expression.Expression is InvocationExpressionSyntax invocation &&
                    invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression is MyBaseExpressionSyntax myBase &&
                    memberAccess.Name.Identifier.Text.Equals("New", System.StringComparison.OrdinalIgnoreCase) &&
                    invocation.ArgumentList.Arguments.Count > maximum);

        private static readonly Dictionary<SyntaxKind, string> mapping = new Dictionary<SyntaxKind, string>
        {
            { SyntaxKind.SubNewStatement, "Constructor" },
            { SyntaxKind.FunctionStatement, "Function" },
            { SyntaxKind.SubStatement, "Sub" },
            { SyntaxKind.DelegateFunctionStatement, "Delegate" },
            { SyntaxKind.DelegateSubStatement, "Delegate" },
            { SyntaxKind.SubLambdaHeader, "Lambda" },
            { SyntaxKind.FunctionLambdaHeader, "Lambda" },
        };

        private static readonly SyntaxKind[] LambdaHeaders = new SyntaxKind[]
        {
            SyntaxKind.FunctionLambdaHeader,
            SyntaxKind.SubLambdaHeader
        };
    }
}
