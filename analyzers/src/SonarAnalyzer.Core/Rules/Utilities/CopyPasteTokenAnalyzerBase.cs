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

using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Core.Rules
{
    public abstract class CopyPasteTokenAnalyzerBase<TSyntaxKind> : UtilityAnalyzerBase<TSyntaxKind, CopyPasteTokenInfo>
        where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S9999-cpd";
        private const string Title = "Copy-paste token calculator";

        protected abstract string GetCpdValue(SyntaxToken token);
        protected abstract bool IsUsingDirective(SyntaxNode node);
        protected override UtilityAnalyzerParameters ReadParameters(IAnalysisContext context) =>
            base.ReadParameters(context) with { AnalyzeTestProjects = false };

        protected sealed override bool AnalyzeUnchangedFiles => true;
        protected sealed override string FileName => "token-cpd.pb";

        protected CopyPasteTokenAnalyzerBase() : base(DiagnosticId, Title) { }

        protected sealed override bool ShouldGenerateMetrics(UtilityAnalyzerParameters parameters, SyntaxTree tree) =>
            !GeneratedCodeRecognizer.IsRazorGeneratedFile(tree)
            && base.ShouldGenerateMetrics(parameters, tree);

        protected sealed override CopyPasteTokenInfo CreateMessage(UtilityAnalyzerParameters parameters, SyntaxTree tree, SemanticModel model)
        {
            var cpdTokenInfo = new CopyPasteTokenInfo { FilePath = tree.FilePath };
            foreach (var token in tree.GetRoot().DescendantTokens(n => !IsUsingDirective(n)))
            {
                if (GetCpdValue(token) is var value && !string.IsNullOrWhiteSpace(value))
                {
                    cpdTokenInfo.TokenInfo.Add(new CopyPasteTokenInfo.Types.TokenInfo { TokenValue = value, TextRange = GetTextRange(Location.Create(tree, token.Span).GetLineSpan()) });
                }
            }
            return cpdTokenInfo;
        }
    }
}
