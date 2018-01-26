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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers
{
    public static class AnalysisContextExtensions
    {
        public static SyntaxTree GetSyntaxTree(this SyntaxNodeAnalysisContext context) => context.Node.SyntaxTree;
        public static SyntaxTree GetSyntaxTree(this SyntaxTreeAnalysisContext context) => context.Tree;
        public static SyntaxTree GetSyntaxTree(this CompilationAnalysisContext context) => context.Compilation.SyntaxTrees.FirstOrDefault();
        public static SyntaxTree GetSyntaxTree(this CompilationStartAnalysisContext context) => context.Compilation.SyntaxTrees.FirstOrDefault();
        public static SyntaxTree GetSyntaxTree(this SymbolAnalysisContext context) => context.Symbol.Locations.FirstOrDefault(l => l.SourceTree != null)?.SourceTree;
        public static SyntaxTree GetSyntaxTree(this CodeBlockAnalysisContext context) => context.CodeBlock.SyntaxTree;
        public static SyntaxTree GetSyntaxTree<TLanguageKindEnum>(this CodeBlockStartAnalysisContext<TLanguageKindEnum> context)
            where TLanguageKindEnum : struct
        {
            return context.CodeBlock.SyntaxTree;
        }
        public static SyntaxTree GetSyntaxTree(this SemanticModelAnalysisContext context) => context.SemanticModel.SyntaxTree;
    }
}
