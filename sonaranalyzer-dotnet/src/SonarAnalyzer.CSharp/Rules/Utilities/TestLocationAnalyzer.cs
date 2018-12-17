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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestLocationAnalyzer : TestLocationAnalyzerBase
    {
        protected override IEnumerable<INamedTypeSymbol> GetNamedTypes(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            var collector = new ClassDeclarationCollector();
            collector.Visit(syntaxTree.GetRoot());

            return collector.ClassDeclarations.Select(classDeclaration => semanticModel.GetDeclaredSymbol(classDeclaration));
        }

        private class ClassDeclarationCollector : CSharpSyntaxWalker
        {
            public List<ClassDeclarationSyntax> ClassDeclarations { get; } = new List<ClassDeclarationSyntax>();

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                ClassDeclarations.Add(node);
                return;
            }

            public override void VisitStructDeclaration(StructDeclarationSyntax node)
            {
            }

            public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
            {
            }

            public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
            {
            }
        }
    }
}
