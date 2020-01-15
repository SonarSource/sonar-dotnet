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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class SwitchCasesMinimumThree : SwitchCasesMinimumThreeBase
    {
        private enum SwitchExpressionType
        {
            SingleReturnValue,
            TwoReturnValues,
            ManyReturnValues
        }

        private const string SwitchStatementMessage = "Replace this 'switch' statement with 'if' statements to increase readability.";

        private const string TwoReturnValueSwitchExpressionMessage = "Replace this 'switch' expression with a ternary conditional operator to increase readability.";

        private const string SingleReturnValueSwitchExpressionMessage = "Remove this 'switch' expression to increase readability.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, "{0}", RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var switchNode = (SwitchStatementSyntax)c.Node;
                    if (!HasAtLeastThreeLabels(switchNode))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, switchNode.SwitchKeyword.GetLocation(), SwitchStatementMessage));
                    }
                },
                SyntaxKind.SwitchStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var switchNode = (SwitchExpressionSyntaxWrapper)c.Node;
                    var message = EvaluateType(switchNode) switch
                    {
                        SwitchExpressionType.SingleReturnValue => SingleReturnValueSwitchExpressionMessage,
                        SwitchExpressionType.TwoReturnValues => TwoReturnValueSwitchExpressionMessage,
                        _ => string.Empty
                    };
                    if (message != string.Empty)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, switchNode.SwitchKeyword.GetLocation(), message));
                    }
                },
                SyntaxKindEx.SwitchExpression);
        }

        private static bool HasAtLeastThreeLabels(SwitchStatementSyntax node) =>
            node.Sections.Sum(section => section.Labels.Count) >= 3;

        private static SwitchExpressionType EvaluateType(SwitchExpressionSyntaxWrapper switchExpression)
        {
            var numberOfArms = switchExpression.Arms.Count;
            if (numberOfArms > 2)
            {
                return SwitchExpressionType.ManyReturnValues;
            }
            var hasDiscardValue = switchExpression.Arms.Any(arm => DiscardPatternSyntaxWrapper.IsInstance(arm.Pattern.SyntaxNode));
            if (numberOfArms == 2)
            {
                return hasDiscardValue ? SwitchExpressionType.TwoReturnValues : SwitchExpressionType.ManyReturnValues;
            }
            if (numberOfArms == 1)
            {
                return hasDiscardValue ? SwitchExpressionType.SingleReturnValue : SwitchExpressionType.TwoReturnValues;
            }
            return SwitchExpressionType.SingleReturnValue;
        }
    }
}
