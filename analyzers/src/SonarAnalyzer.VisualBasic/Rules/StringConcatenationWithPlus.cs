/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Rules
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
                    var leftType = c.Model.GetTypeInfo(binary.Left).Type;
                    if (leftType.Is(KnownType.System_String)
                        // If op_Addition exist, there's areason for it => don't raise. We don't care about type of op_Addition arguments, they match because it compiles.
                        || (leftType.GetMembers("op_Addition").IsEmpty && c.Model.GetTypeInfo(binary.Right).Type.Is(KnownType.System_String)))
                    {
                        c.ReportIssue(Rule, binary.OperatorToken);
                    }
                },
                SyntaxKind.AddExpression);
    }
}
