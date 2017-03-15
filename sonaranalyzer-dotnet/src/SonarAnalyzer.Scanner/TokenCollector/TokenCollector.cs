/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Classification;
using SonarAnalyzer.Protobuf;
using System.Collections.Immutable;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Runner
{
    public class TokenCollector
    {

        private readonly SyntaxTree tree;
        private readonly SemanticModel semanticModel;

        private readonly string filePath;

        public TokenCollector(string filePath, Document document)
        {
            this.filePath = filePath;
            this.tree = document.GetSyntaxTreeAsync().Result;
            this.semanticModel = document.GetSemanticModelAsync().Result;
        }

        public SymbolReferenceInfo SymbolReferenceInfo
        {
            get
            {
                var analyzer = tree.GetRoot().Language == LanguageNames.CSharp
                    ? (Rules.SymbolReferenceAnalyzerBase)new Rules.CSharp.SymbolReferenceAnalyzer()
                    : new Rules.VisualBasic.SymbolReferenceAnalyzer();

                var message = Rules.SymbolReferenceAnalyzerBase.CalculateSymbolReferenceInfo(tree, semanticModel, 
                    t => IsIdentifier(t), t => analyzer.GetBindableParent(t), s => analyzer.GetSetKeyword(s));
                message.FilePath = filePath;
                return message;
            }
        }


        public TokenTypeInfo TokenTypeInfo
        {
            get
            {
                var analyzer = tree.GetRoot().Language == LanguageNames.CSharp
                    ? (Rules.TokenTypeAnalyzerBase)new Rules.CSharp.TokenTypeAnalyzer()
                    : new Rules.VisualBasic.TokenTypeAnalyzer();

                var message = analyzer.GetTokenTypeInfo(tree, semanticModel);
                message.FilePath = filePath;
                return message;
            }
        }

        public CopyPasteTokenInfo CopyPasteTokenInfo
        {
            get
            {
                var analyzer = tree.GetRoot().Language == LanguageNames.CSharp
                    ? (Rules.CopyPasteTokenAnalyzerBase)new Rules.CSharp.CopyPasteTokenAnalyzer()
                    : new Rules.VisualBasic.CopyPasteTokenAnalyzer();

                var message = analyzer.CalculateTokenInfo(tree);
                message.FilePath = filePath;
                return message;
            }
        }

        private static bool IsIdentifier(SyntaxToken token)
        {
            return token.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.IdentifierToken) ||
                token.IsKind(Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.IdentifierToken);
        }
    }
}
