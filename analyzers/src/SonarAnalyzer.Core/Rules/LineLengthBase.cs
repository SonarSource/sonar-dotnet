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

namespace SonarAnalyzer.Core.Rules
{
    public abstract class LineLengthBase : ParametrizedDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S103";
        internal const string MessageFormat = "Split this {1} characters long line (which is greater than {0} authorized).";

        private const int DefaultValueMaximum = 200;

        [RuleParameter("maximumLineLength", PropertyType.Integer, "The maximum authorized line length.",
            DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected sealed override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterTreeAction(
                GeneratedCodeRecognizer,
                c =>
                {
                    foreach (var line in c.Tree.GetText().Lines
                        .Where(line => line.Span.Length > Maximum))
                    {
                        c.ReportIssue(SupportedDiagnostics[0], c.Tree.GetLocation(line.Span), Maximum.ToString(), line.Span.Length.ToString());
                    }
                });
        }
    }
}
