/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
        private static readonly DiagnosticDescriptor rule =
           DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override DiagnosticDescriptor Rule { get; } = rule;

        protected override bool IsCaseSensitive => false;

        protected override SyntaxToken? GetNameSyntaxNode(StatementSyntax member)
        {
            if (member is ConstructorBlockSyntax constructorDeclaration)
            {
                return constructorDeclaration.SubNewStatement.NewKeyword;
            }
            else if (member is MethodBlockSyntax methodDeclaration)
            {
                return methodDeclaration.SubOrFunctionStatement.Identifier;
            }
            else if (member is MethodStatementSyntax methodStatement)
            {
                return methodStatement.Identifier;
            }
            return null;
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var classDeclaration = (ClassBlockSyntax)c.Node;
                CheckMembers(c, classDeclaration.Members);
            },
            SyntaxKind.ClassBlock);

            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var interfaceDeclaration = (InterfaceBlockSyntax)c.Node;
                CheckMembers(c, interfaceDeclaration.Members);
            },
            SyntaxKind.InterfaceBlock);

            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var structureDeclaration = (StructureBlockSyntax)c.Node;
                CheckMembers(c, structureDeclaration.Members);
            },
            SyntaxKind.StructureBlock);
        }
    }
}
