/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class FileLinesBase : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S104";
        internal const string MessageFormat = "This file has {1} lines, which is greater than {0} authorized. Split it into " +
            "smaller files.";

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private const int DefaultValueMaximum = 1000;

        [RuleParameter("maximumFileLocThreshold", PropertyType.Integer, "Maximum authorized lines in a file.",
            DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxTreeActionInNonGenerated(
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
                        stac.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, Location.Create(stac.Tree,
                            TextSpan.FromBounds(0, 0)), Maximum, linesCount));
                    }
                });
        }

        protected abstract DiagnosticDescriptor Rule { get; }
        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract bool IsEndOfFileToken(SyntaxToken token);
    }
}
