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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class InsecureEncryptionAlgorithm : DoNotCallInsecureSecurityAlgorithmBase<SyntaxKind, InvocationExpressionSyntax, ObjectCreationExpressionSyntax>
    {
        private const string DiagnosticId = "S5547";
        private const string MessageFormat = "Use a strong cipher algorithm.";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw new System.NotImplementedException();

        protected override ISet<string> AlgorithmParameterlessFactoryMethods => throw new System.NotImplementedException();

        protected override ISet<string> AlgorithmParameterizedFactoryMethods => throw new System.NotImplementedException();

        protected override ISet<string> FactoryParameterNames => throw new System.NotImplementedException();

        protected override SyntaxKind ObjectCreation => throw new System.NotImplementedException();

        protected override SyntaxKind Invocation => throw new System.NotImplementedException();

        protected override SyntaxKind StringLiteral => throw new System.NotImplementedException();

        protected override ILanguageFacade LanguageFacade => throw new System.NotImplementedException();

        private protected override ImmutableArray<KnownType> AlgorithmTypes => throw new System.NotImplementedException();

        protected override SyntaxNode InvocationExpression(InvocationExpressionSyntax invocation) => throw new System.NotImplementedException();
        protected override bool IsInsecureBaseAlgorithmCreationFactoryCall(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocationExpression) => throw new System.NotImplementedException();
        protected override Location Location(ObjectCreationExpressionSyntax objectCreation) => throw new System.NotImplementedException();
    }
}
