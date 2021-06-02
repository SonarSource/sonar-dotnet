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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Wrappers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class MethodsShouldNotHaveIdenticalImplementations : MethodsShouldNotHaveIdenticalImplementationsBase<IMethodDeclaration, SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override IEnumerable<IMethodDeclaration> GetMethodDeclarations(SyntaxNode node) =>
            ((TypeDeclarationSyntax)node).GetMethodDeclarations();

        protected override bool AreDuplicates(IMethodDeclaration firstMethod, IMethodDeclaration secondMethod) =>
            firstMethod.Body != null
            && firstMethod.Body.Statements.Count > 1
            && firstMethod.Identifier.ValueText != secondMethod.Identifier.ValueText
            && HaveSameParameters<ParameterSyntax>(firstMethod.ParameterList.Parameters, secondMethod.ParameterList.Parameters)
            && firstMethod.Body.IsEquivalentTo(secondMethod.Body, false);

        protected override SyntaxToken GetMethodIdentifier(IMethodDeclaration method) => method.Identifier;
    }
}
