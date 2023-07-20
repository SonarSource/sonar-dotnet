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

namespace SonarAnalyzer.Rules
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
                        c.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], c.Tree.GetLocation(line.Span),
                            Maximum, line.Span.Length));
                    }
                });
        }
    }
}
