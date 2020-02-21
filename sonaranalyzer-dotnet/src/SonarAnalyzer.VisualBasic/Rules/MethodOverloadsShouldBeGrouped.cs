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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class MethodOverloadsShouldBeGrouped : MethodOverloadsShouldBeGroupedBase<StatementSyntax>
    {
        public MethodOverloadsShouldBeGrouped() : base(RspecStrings.ResourceManager) { }

        protected override MemberInfo CreateMemberInfo(SyntaxNodeAnalysisContext c, StatementSyntax member)
        {
            if (member is ConstructorBlockSyntax constructor)
            {
                return new MemberInfo(c, member, constructor.SubNewStatement.NewKeyword, IsStaticStatement(constructor.SubNewStatement), false, false);
            }
            else if (member is MethodBlockSyntax methodBlock)
            {
                return new MemberInfo(c, member, methodBlock.SubOrFunctionStatement.Identifier, IsStaticStatement(methodBlock.SubOrFunctionStatement), IsAbstractStatement(methodBlock.SubOrFunctionStatement), false);
            }
            else if (member is MethodStatementSyntax methodStatement)
            {
                return new MemberInfo(c, member, methodStatement.Identifier, IsStaticStatement(methodStatement), IsAbstractStatement(methodStatement), false);
            }
            return null;
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var typeDeclaration = (TypeBlockSyntax)c.Node;
                CheckMembers(c, typeDeclaration.Members);
            },
            SyntaxKind.ClassBlock,
            SyntaxKind.InterfaceBlock,
            SyntaxKind.StructureBlock);
        }

        private static bool IsStaticStatement(MethodBaseSyntax statement) => statement.DescendantTokens().Any(x => x.Kind() == SyntaxKind.SharedKeyword);

        private static bool IsAbstractStatement(MethodBaseSyntax statement) => statement.DescendantTokens().Any(x => x.Kind() == SyntaxKind.MustOverrideKeyword);

    }
}
