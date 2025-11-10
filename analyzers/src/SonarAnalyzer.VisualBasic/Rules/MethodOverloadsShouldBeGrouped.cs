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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class MethodOverloadsShouldBeGrouped : MethodOverloadsShouldBeGroupedBase<SyntaxKind, StatementSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
        {
            SyntaxKind.ClassBlock,
            SyntaxKind.InterfaceBlock,
            SyntaxKind.StructureBlock
        };

        protected override MemberInfo CreateMemberInfo(SonarSyntaxNodeReportingContext c, StatementSyntax member)
        {
            if (member is ConstructorBlockSyntax constructor)
            {
                return new MemberInfo(c, member, constructor.SubNewStatement.NewKeyword, IsStaticStatement(constructor.SubNewStatement), false, false);
            }
            else if (member is MethodBlockSyntax methodBlock)
            {
                return new MemberInfo(c,
                                      member,
                                      methodBlock.SubOrFunctionStatement.Identifier,
                                      IsStaticStatement(methodBlock.SubOrFunctionStatement),
                                      IsAbstractStatement(methodBlock.SubOrFunctionStatement),
                                      false);
            }
            else if (member is MethodStatementSyntax methodStatement)
            {
                return new MemberInfo(c, member, methodStatement.Identifier, IsStaticStatement(methodStatement), IsAbstractStatement(methodStatement), false);
            }
            return null;
        }

        protected override IEnumerable<StatementSyntax> GetMemberDeclarations(SyntaxNode node) =>
            ((TypeBlockSyntax)node).Members;

        private static bool IsStaticStatement(MethodBaseSyntax statement) =>
            statement.DescendantTokens().Any(x => x.Kind() == SyntaxKind.SharedKeyword);

        private static bool IsAbstractStatement(MethodBaseSyntax statement) =>
            statement.DescendantTokens().Any(x => x.Kind() == SyntaxKind.MustOverrideKeyword);
    }
}
