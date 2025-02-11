/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
    public sealed class DoNotUseByVal : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3860";
        private const string MessageFormat = "Remove this redundant 'ByVal' modifier.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
            {
                var parameter = (ParameterSyntax)c.Node;
                foreach (var byVal in parameter.Modifiers.Where(IsByVal))
                {
                    c.ReportIssue(rule, byVal);
                }
            },
            SyntaxKind.Parameter);
        }

        public static bool IsByVal(SyntaxToken modifier)
        {
            return modifier.IsKind(SyntaxKind.ByValKeyword);
        }
    }
}
