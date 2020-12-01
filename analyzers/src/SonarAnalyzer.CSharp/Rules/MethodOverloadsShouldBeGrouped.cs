/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class MethodOverloadsShouldBeGrouped : MethodOverloadsShouldBeGroupedBase<MemberDeclarationSyntax>
    {
        public MethodOverloadsShouldBeGrouped() : base(RspecStrings.ResourceManager) { }

        protected override MemberInfo CreateMemberInfo(SyntaxNodeAnalysisContext c, MemberDeclarationSyntax member)
        {
            if (!IsValidMemberForOverload(member))
            {
                return null;
            }
            if (member is ConstructorDeclarationSyntax constructor)
            {
                return new MemberInfo(c, member, constructor.Identifier, IsStatic(constructor), false, true);
            }
            else if (member is MethodDeclarationSyntax method)
            {
                return new MemberInfo(c, member, method.Identifier, IsStatic(method), method.Modifiers.Any(x => x.Kind() == SyntaxKind.AbstractKeyword), true);
            }
            return null;
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var typeDeclaration = (TypeDeclarationSyntax)c.Node;
                CheckMembers(c, typeDeclaration.Members);
            },
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.StructDeclaration);
        }

        private static bool IsValidMemberForOverload(MemberDeclarationSyntax member)
        {
            if (member is MethodDeclarationSyntax methodDeclaration)
            {
                return methodDeclaration.ExplicitInterfaceSpecifier == null;
            }
            return true;
        }

        private static bool IsStatic(BaseMethodDeclarationSyntax declaration) => declaration.Modifiers.Any(x => x.Kind() == SyntaxKind.StaticKeyword);

    }
}
