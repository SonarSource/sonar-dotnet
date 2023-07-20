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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Rules
{
    public abstract class FileLinesBase : ParametrizedDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S104";
        internal const string MessageFormat = "This file has {1} lines, which is greater than {0} authorized. Split it into " +
            "smaller files.";

        private const int DefaultValueMaximum = 1000;

        [RuleParameter("maximumFileLocThreshold", PropertyType.Integer, "Maximum authorized lines in a file.",
            DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        protected override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterTreeAction(
                GeneratedCodeRecognizer,
                stac =>
                {
                    var linesCount = stac.Tree
                        .GetRoot()
                        .DescendantTokens()
                        .Where(token => !IsEndOfFileToken(token))
                        .SelectMany(token => token.GetLineNumbers(isZeroBasedCount: false))
                        .Distinct()
                        .LongCount();

                    if (linesCount > Maximum)
                    {
                        stac.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], Location.Create(stac.Tree,
                            TextSpan.FromBounds(0, 0)), Maximum, linesCount));
                    }
                });
        }

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract bool IsEndOfFileToken(SyntaxToken token);
    }
}
