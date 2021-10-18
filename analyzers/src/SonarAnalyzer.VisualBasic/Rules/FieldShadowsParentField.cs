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
    [Rule(S2387DiagnosticId)]
    [Rule(S4025DiagnosticId)]
    public sealed class FieldShadowsParentField : FieldShadowsParentFieldBase<ModifiedIdentifierSyntax>
    {
        public FieldShadowsParentField() : base(RspecStrings.ResourceManager) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax)c.Node;
                    if (!fieldDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.ShadowsKeyword)))
                    {
                        foreach (var diagnostics in fieldDeclaration.Declarators.SelectMany(x => x.Names).SelectMany(x => CheckFields(c.SemanticModel, x)))
                        {
                            c.ReportIssue(diagnostics, context);
                        }
                    }
                },
                SyntaxKind.FieldDeclaration);

        protected override SyntaxToken GetIdentifier(ModifiedIdentifierSyntax declarator) =>
            declarator.Identifier;
    }
}
