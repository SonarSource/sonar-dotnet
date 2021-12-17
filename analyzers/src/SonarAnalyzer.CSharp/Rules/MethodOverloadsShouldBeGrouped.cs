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

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MethodOverloadsShouldBeGrouped : MethodOverloadsShouldBeGroupedBase<SyntaxKind, MemberDeclarationSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKindEx.RecordClassDeclaration
        };

        protected override MemberInfo CreateMemberInfo(SyntaxNodeAnalysisContext c, MemberDeclarationSyntax member)
        {
            if (!IsValidMemberForOverload(member))
            {
                return null;
            }
            if (member is ConstructorDeclarationSyntax constructor)
            {
                return new MemberInfo(c, member, constructor.Identifier, constructor.IsStatic(), false, true);
            }
            else if (member is MethodDeclarationSyntax method)
            {
                return new MemberInfo(c, member, method.Identifier, method.IsStatic(), method.Modifiers.Any(SyntaxKind.AbstractKeyword), true);
            }
            return null;
        }

        protected override IEnumerable<MemberDeclarationSyntax> GetMemberDeclarations(SyntaxNode node) =>
            ((TypeDeclarationSyntax)node).Members;

        private static bool IsValidMemberForOverload(MemberDeclarationSyntax member) =>
            (member as MethodDeclarationSyntax)?.ExplicitInterfaceSpecifier == null;
    }
}
