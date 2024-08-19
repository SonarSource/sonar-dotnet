/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
            SyntaxKindEx.RecordDeclaration,
            SyntaxKindEx.RecordStructDeclaration,
        };

        protected override MemberInfo CreateMemberInfo(SonarSyntaxNodeReportingContext c, MemberDeclarationSyntax member) =>
            member switch
            {
                ConstructorDeclarationSyntax constructor => new MemberInfo(c, member, constructor.Identifier, constructor.IsStatic(), false, true),
                MethodDeclarationSyntax { ExplicitInterfaceSpecifier: { } } => null, // Skip explicit interface implementations
                MethodDeclarationSyntax method => new MemberInfo(c, member, method.Identifier, method.IsStatic(), method.Modifiers.Any(SyntaxKind.AbstractKeyword), true),
                _ => null,
            };

        protected override IEnumerable<MemberDeclarationSyntax> GetMemberDeclarations(SyntaxNode node) =>
            ((TypeDeclarationSyntax)node).Members;
    }
}
