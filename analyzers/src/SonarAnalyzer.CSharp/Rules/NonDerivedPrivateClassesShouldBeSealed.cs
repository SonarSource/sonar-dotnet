﻿/*
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class NonDerivedPrivateClassesShouldBeSealed : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3260";
        private const string MessageFormat = "Private classes which are not derived in the current assembly should be marked as 'sealed'.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var classDeclarationSyntax = (ClassDeclarationSyntax)c.Node;
                if (IsPrivateButNotSealedClass(classDeclarationSyntax))
                    {
                    var nestedPrivateClassInfo = c.SemanticModel.GetDeclaredSymbol(c.Node);

                    if (!PrivateClassIsInheritedByAnotherClass(nestedPrivateClassInfo))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, classDeclarationSyntax.Identifier.GetLocation()));
                    }
                }
            },
            SyntaxKind.ClassDeclaration);

        private static bool IsPrivateButNotSealedClass(ClassDeclarationSyntax classDeclaration) =>
           classDeclaration.Modifiers.Any(SyntaxKind.PrivateKeyword) && !classDeclaration.Modifiers.Any(SyntaxKind.SealedKeyword);

        private static bool PrivateClassIsInheritedByAnotherClass(ITypeSymbol privateClassInfo) =>
            privateClassInfo.ContainingType
                .GetMembers()
                .Where(s => s.Kind == SymbolKind.NamedType && !s.Name.Equals(privateClassInfo.Name))
                .Select(x => (x as ITypeSymbol))
                .Any(c => c.BaseType.Equals(privateClassInfo));
    }
}
