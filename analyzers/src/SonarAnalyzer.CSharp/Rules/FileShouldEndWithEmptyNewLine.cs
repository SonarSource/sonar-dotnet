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

using System.IO;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FileShouldEndWithEmptyNewLine : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S113";
        private const string MessageFormat = "Add a new line at the end of the file '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterTreeAction(stac =>
                {
                    var lastToken = stac.Tree.GetRoot().GetLastToken();

                    if (lastToken == default(SyntaxToken))
                    {
                        return;
                    }

                    var isFileEndingWithNewLine = lastToken.TrailingTrivia.LastOrDefault().IsKind(SyntaxKind.EndOfLineTrivia);
                    if (!isFileEndingWithNewLine)
                    {
                        stac.ReportIssue(CreateDiagnostic(Rule, lastToken.GetLocation(), Path.GetFileName(stac.Tree.FilePath)));
                    }
                });
    }
}
