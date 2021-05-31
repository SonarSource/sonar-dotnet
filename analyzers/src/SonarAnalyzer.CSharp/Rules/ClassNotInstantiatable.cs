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
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ClassNotInstantiatable : ClassNotInstantiatableBase<BaseTypeDeclarationSyntax, SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
        protected override GeneratedCodeRecognizer CodeRecognizer => CSharpGeneratedCodeRecognizer.Instance;

        protected override bool IsTypeDeclaration(SyntaxNode node) =>
            node.IsAnyKind(SyntaxKind.ClassDeclaration, SyntaxKindEx.RecordDeclaration);

        protected override IEnumerable<Tuple<SyntaxNodeAndSemanticModel<BaseTypeDeclarationSyntax>, Diagnostic>> CollectRemovableDeclarations(INamedTypeSymbol namedType, Compilation compilation,
            int count)
        {
            var typeDeclarations = new CSharpRemovableDeclarationCollector(namedType, compilation).TypeDeclarations;

            var message = count > 1
                ? "at least one of its constructors"
                : "its constructor";

            return typeDeclarations
                .Select(x => new Tuple<SyntaxNodeAndSemanticModel<BaseTypeDeclarationSyntax>, Diagnostic>(x, Diagnostic.Create(rule,
                                                                                                                               x.SyntaxNode.Identifier.GetLocation(),
                                                                                                                               DeclarationKind(x.SyntaxNode),
                                                                                                                               message)));
        }

        private static string DeclarationKind(SyntaxNode node) =>
            node.IsKind(SyntaxKind.ClassDeclaration)
                ? "class"
                : "record";
    }
}
