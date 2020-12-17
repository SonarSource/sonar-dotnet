/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } = VisualBasicGeneratedCodeRecognizer.Instance;
        protected override SyntaxKind[] SyntaxKinds { get; } = new SyntaxKind[] { SyntaxKind.ParameterList };

        private static readonly ImmutableDictionary<SyntaxKind, string> NodeToDeclarationName = new Dictionary<SyntaxKind, string>
        {
            { SyntaxKind.SubNewStatement, "Constructor" },
            { SyntaxKind.FunctionStatement, "Function" },
            { SyntaxKind.SubStatement, "Sub" },
            { SyntaxKind.DelegateFunctionStatement, "Delegate" },
            { SyntaxKind.DelegateSubStatement, "Delegate" },
            { SyntaxKind.SubLambdaHeader, "Lambda" },
            { SyntaxKind.FunctionLambdaHeader, "Lambda" },
            { SyntaxKind.PropertyStatement, "Property" },
            { SyntaxKind.EventStatement, "Event" },
        }.ToImmutableDictionary();

        private static readonly SyntaxKind[] LambdaHeaders = new[]
        {
            SyntaxKind.FunctionLambdaHeader,
            SyntaxKind.SubLambdaHeader
        };

        public TooManyParameters() : base(RspecStrings.ResourceManager) { }

        protected override string UserFriendlyNameForNode(SyntaxNode node) => NodeToDeclarationName[node.Kind()];

        protected override int CountParameters(ParameterListSyntax parameterList) => parameterList.Parameters.Count;

        protected override bool CanBeChanged(SyntaxNode node, SemanticModel semanticModel) =>
            node.IsAnyKind(LambdaHeaders)
            || (NodeToDeclarationName.ContainsKey(node.Kind()) && VerifyCanBeChangedBySymbol(node, semanticModel));

        protected override int BaseParameterCount(SyntaxNode node) =>
            node.Parent is ConstructorBlockSyntax constructorBlock
            && constructorBlock.SubNewStatement.ParameterList?.Parameters.Count > Maximum   // Performance optimalization
                ? constructorBlock.Statements.Select(x => MyBaseNewParameterCount(x)).SingleOrDefault(x => x > 0)
                : 0;

        private static int MyBaseNewParameterCount(StatementSyntax statement) =>
            statement is ExpressionStatementSyntax expression
            && expression.Expression is InvocationExpressionSyntax invocation
            && invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Expression is MyBaseExpressionSyntax
            && memberAccess.Name.Identifier.Text.Equals("New", System.StringComparison.OrdinalIgnoreCase)
            ? invocation.ArgumentList.Arguments.Count
            : 0;
    }
}
