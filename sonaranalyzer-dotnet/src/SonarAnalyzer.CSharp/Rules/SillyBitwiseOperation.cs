/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Immutable;
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
    public sealed class SillyBitwiseOperation : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2437";
        private const string MessageFormat = "Remove this silly bit operation.";
        internal const string IsReportingOnLeftKey = "IsReportingOnLeft";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                fadeOutCode: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckBinary(c, -1),
                SyntaxKind.BitwiseAndExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckBinary(c, 0),
                SyntaxKind.BitwiseOrExpression,
                SyntaxKind.ExclusiveOrExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckAssignment(c, -1),
                SyntaxKind.AndAssignmentExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckAssignment(c, 0),
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression);
        }

        private static void CheckAssignment(SyntaxNodeAnalysisContext context, int constValueToLookFor)
        {
            var assignment = (AssignmentExpressionSyntax)context.Node;
            if (ExpressionNumericConverter.TryGetConstantIntValue(assignment.Right, out var constValue) &&
                constValue == constValueToLookFor)
            {
                var location = assignment.Parent is StatementSyntax
                    ? assignment.Parent.GetLocation()
                    : assignment.OperatorToken.CreateLocation(assignment.Right);
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location));
            }
        }

        private static void CheckBinary(SyntaxNodeAnalysisContext context, int constValueToLookFor)
        {
            var binary = (BinaryExpressionSyntax) context.Node;
            if (ExpressionNumericConverter.TryGetConstantIntValue(binary.Left, out var constValue) &&
                constValue == constValueToLookFor)
            {
                var location = binary.Left.CreateLocation(binary.OperatorToken);
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location, ImmutableDictionary<string, string>.Empty.Add(IsReportingOnLeftKey, true.ToString())));
                return;
            }

            if (ExpressionNumericConverter.TryGetConstantIntValue(binary.Right, out constValue) &&
                constValue == constValueToLookFor)
            {
                var location = binary.OperatorToken.CreateLocation(binary.Right);
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location, ImmutableDictionary<string, string>.Empty.Add(IsReportingOnLeftKey, false.ToString())));
            }
        }
    }
}
