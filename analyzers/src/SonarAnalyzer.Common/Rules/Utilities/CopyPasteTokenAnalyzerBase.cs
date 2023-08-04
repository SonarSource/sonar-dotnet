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

using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules
{
    public abstract class CopyPasteTokenAnalyzerBase<TSyntaxKind> : UtilityAnalyzerBase<TSyntaxKind, CopyPasteTokenInfo>
        where TSyntaxKind : struct
    {
        private const string DiagnosticId = "S9999-cpd";
        private const string Title = "Copy-paste token calculator";

        protected abstract string GetCpdValue(SyntaxToken token);
        protected abstract bool IsUsingDirective(SyntaxNode node);

        protected sealed override bool AnalyzeTestProjects => false;
        protected sealed override bool AnalyzeUnchangedFiles => true;
        protected sealed override string FileName => "token-cpd.pb";

        protected CopyPasteTokenAnalyzerBase() : base(DiagnosticId, Title) { }

        protected sealed override CopyPasteTokenInfo CreateMessage(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            var cpdTokenInfo = new CopyPasteTokenInfo { FilePath = GetFilePath(syntaxTree) };
            foreach (var token in syntaxTree.GetRoot().DescendantTokens(n => !IsUsingDirective(n)))
            {
                if (GetCpdValue(token) is var value
                    && !string.IsNullOrWhiteSpace(value)
                    && token.GetLocation().TryEnsureMappedLocation(out var mappedLocation)) // ToDo: location is not mapped correctly due to: https://github.com/dotnet/roslyn/issues/69248
                {
                    cpdTokenInfo.TokenInfo.Add(new CopyPasteTokenInfo.Types.TokenInfo { TokenValue = value, TextRange = GetTextRange(mappedLocation.GetLineSpan()) });
                }
            }
            return cpdTokenInfo;
        }
    }
}
