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

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class NonFlagsEnumInBitwiseOperation : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3265";
        private const string MessageFormat = "{0}";
        internal const string MessageRemove = "Remove this bitwise operation; the enum '{0}' is not marked with 'Flags' attribute.";
        internal const string MessageChangeOrRemove = "Mark enum '{0}' with 'Flags' attribute or remove this bitwise operation.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckExpressionWithOperator<BinaryExpressionSyntax>(b => b.OperatorToken, c),
                SyntaxKind.BitwiseOrExpression,
                SyntaxKind.BitwiseAndExpression,
                SyntaxKind.ExclusiveOrExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckExpressionWithOperator<AssignmentExpressionSyntax>(a => a.OperatorToken, c),
                SyntaxKind.AndAssignmentExpression,
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression);
        }

        private static void CheckExpressionWithOperator<T>(Func<T, SyntaxToken> operatorSelector, SyntaxNodeAnalysisContext context)
            where T : SyntaxNode
        {
            if (!(context.SemanticModel.GetSymbolInfo(context.Node).Symbol is IMethodSymbol operation) ||
                operation.MethodKind != MethodKind.BuiltinOperator ||
                operation.ReturnType == null ||
                operation.ReturnType.TypeKind != TypeKind.Enum)
            {
                return;
            }

            if (!operation.ReturnType.HasAttribute(KnownType.System_FlagsAttribute))
            {
                var friendlyTypeName = operation.ReturnType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart);
                var messageFormat = operation.ReturnType.DeclaringSyntaxReferences.Any()
                    ? MessageChangeOrRemove
                    : MessageRemove;

                var message = string.Format(messageFormat, friendlyTypeName);

                var op = operatorSelector((T)context.Node);
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, op.GetLocation(), message));
            }
        }
    }
}
