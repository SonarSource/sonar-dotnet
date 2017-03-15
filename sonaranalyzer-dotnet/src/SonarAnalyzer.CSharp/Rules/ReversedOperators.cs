/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
    public class ReversedOperators : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2757";
        private const string MessageFormat = "Was '{0}' meant instead?";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected sealed override DiagnosticDescriptor Rule => rule;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var unaryExpression = (PrefixUnaryExpressionSyntax) c.Node;

                    var op = unaryExpression.OperatorToken;
                    var prevToken = op.GetPreviousToken();

                    var opLocation = op.GetLocation();
                    var opStartPosition = opLocation.GetLineSpan().StartLinePosition;
                    var prevStartPosition = prevToken.GetLocation().GetLineSpan().StartLinePosition;

                    if (prevToken.IsKind(SyntaxKind.EqualsToken) &&
                        prevStartPosition.Line == opStartPosition.Line &&
                        prevStartPosition.Character == opStartPosition.Character - 1)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, opLocation, $"{op.Text}="));
                    }
                },
                SyntaxKind.UnaryMinusExpression,
                SyntaxKind.UnaryPlusExpression,
                SyntaxKind.LogicalNotExpression);
        }
    }
}
