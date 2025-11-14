/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.IO;

namespace SonarAnalyzer.CSharp.Rules
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
                        stac.ReportIssue(Rule, lastToken, Path.GetFileName(stac.Tree.FilePath));
                    }
                });
    }
}
