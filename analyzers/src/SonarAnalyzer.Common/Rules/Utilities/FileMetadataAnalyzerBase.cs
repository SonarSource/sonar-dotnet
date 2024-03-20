/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules
{
    public abstract class FileMetadataAnalyzerBase<TSyntaxKind> : UtilityAnalyzerBase<TSyntaxKind, FileMetadataInfo>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S9999-metadata";
        private const string Title = "File metadata generator";

        protected sealed override string FileName => "file-metadata.pb";
        protected override UtilityAnalyzerParameters ReadParameters<T>(SonarAnalysisContextBase<T> context) =>
            base.ReadParameters(context) with { AnalyzeGeneratedCode = true };

        protected FileMetadataAnalyzerBase() : base(DiagnosticId, Title) { }

        protected override bool ShouldGenerateMetrics(UtilityAnalyzerParameters parameters, SyntaxTree tree) =>
            !GeneratedCodeRecognizer.IsRazorGeneratedFile(tree)
            && base.ShouldGenerateMetrics(parameters, tree);

        protected sealed override FileMetadataInfo CreateMessage(UtilityAnalyzerParameters parameters, SyntaxTree tree, SemanticModel model) =>
            new()
            {
                FilePath = tree.FilePath,
                IsGenerated = tree.IsGenerated(Language.GeneratedCodeRecognizer),
                Encoding = tree.Encoding?.WebName.ToLowerInvariant() ?? string.Empty
            };
    }
}
