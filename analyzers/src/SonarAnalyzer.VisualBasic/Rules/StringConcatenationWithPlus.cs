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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class StringConcatenationWithPlus : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1645";
        private const string MessageFormat = "Switch this use of the '+' operator to the '&'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var binary = (BinaryExpressionSyntax)c.Node;
                    var leftType = c.SemanticModel.GetTypeInfo(binary.Left).Type;
                    if (leftType.Is(KnownType.System_String)
                        // If op_Addition exist, there's areason for it => don't raise. We don't care about type of op_Addition arguments, they match because it compiles.
                        || (leftType.GetMembers("op_Addition").IsEmpty && c.SemanticModel.GetTypeInfo(binary.Right).Type.Is(KnownType.System_String)))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, binary.OperatorToken.GetLocation()));
                    }
                },
                SyntaxKind.AddExpression);
    }
}
