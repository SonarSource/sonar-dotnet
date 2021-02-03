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

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Helpers.Facade;

namespace SonarAnalyzer.Helpers
{
    internal sealed class VisualBasicFacade : ILanguageFacade<SyntaxKind>
    {
        private static readonly Lazy<VisualBasicFacade> Singleton = new Lazy<VisualBasicFacade>(() => new VisualBasicFacade());
        private static readonly Lazy<IExpressionNumericConverter> ExpressionNumericConverterLazy = new Lazy<IExpressionNumericConverter>(() => new VisualBasicExpressionNumericConverter());
        private static readonly Lazy<SyntaxFacade<SyntaxKind>> SyntaxLazy = new Lazy<SyntaxFacade<SyntaxKind>>(() => new VisualBasicSyntaxFacade());
        private static readonly Lazy<ISyntaxKindFacade<SyntaxKind>> SyntaxKindLazy = new Lazy<ISyntaxKindFacade<SyntaxKind>>(() => new VisualBasicSyntaxKindFacade());

        public StringComparison NameComparison => StringComparison.OrdinalIgnoreCase;
        public GeneratedCodeRecognizer GeneratedCodeRecognizer => VisualBasicGeneratedCodeRecognizer.Instance;
        public IExpressionNumericConverter ExpressionNumericConverter => ExpressionNumericConverterLazy.Value;
        public SyntaxFacade<SyntaxKind> Syntax => SyntaxLazy.Value;
        public ISyntaxKindFacade<SyntaxKind> SyntaxKind => SyntaxKindLazy.Value;

        public static VisualBasicFacade Instance => Singleton.Value;

        private VisualBasicFacade() { }

        public IMethodParameterLookup MethodParameterLookup(SyntaxNode invocation, IMethodSymbol methodSymbol) =>
            new VisualBasicMethodParameterLookup((InvocationExpressionSyntax)invocation, methodSymbol);
    }
}
