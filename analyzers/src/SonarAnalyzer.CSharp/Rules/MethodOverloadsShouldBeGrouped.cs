/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
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
