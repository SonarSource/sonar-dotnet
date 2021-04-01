/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules
{
    public abstract class FileMetadataAnalyzerBase : UtilityAnalyzerBase<FileMetadataInfo>
    {
        protected const string DiagnosticId = "S9999-metadata";
        private const string Title = "File metadata generator";
        private const string MetadataFileName = "file-metadata.pb";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetUtilityDescriptor(DiagnosticId, Title);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected sealed override string FileName => MetadataFileName;

        protected override bool AnalyzeGeneratedCode => true;

        protected sealed override FileMetadataInfo CreateMessage(SyntaxTree syntaxTree, SemanticModel semanticModel) =>
            new FileMetadataInfo
            {
                FilePath = syntaxTree.FilePath,
                IsGenerated = GeneratedCodeRecognizer.IsGenerated(syntaxTree),
                Encoding = syntaxTree.Encoding?.WebName.ToLowerInvariant() ?? string.Empty
            };
    }
}
