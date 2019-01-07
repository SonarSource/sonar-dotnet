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

using System.Collections.Generic;
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
    public sealed class FieldsShouldNotBePublic : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1104";
        private const string MessageFormat = "Make this field 'private' and encapsulate it in a 'public' property.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<SyntaxKind> ValidModifiers = new HashSet<SyntaxKind>
        {
            SyntaxKind.PrivateKeyword,
            SyntaxKind.ProtectedKeyword,
            SyntaxKind.InternalKeyword,
            SyntaxKind.ReadOnlyKeyword,
            SyntaxKind.ConstKeyword
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax)c.Node;
                    var parentSymbol = c.SemanticModel.GetDeclaredSymbol(fieldDeclaration.Parent);
                    if (fieldDeclaration.Modifiers.Any(m => ValidModifiers.Contains(m.Kind())) ||
                        parentSymbol.GetAttributes(KnownType.System_Runtime_InteropServices_StructLayoutAttribute).Any())
                    {
                        return;
                    }

                    var firstVariable = fieldDeclaration.Declaration.Variables[0];
                    var symbol = c.SemanticModel.GetDeclaredSymbol(firstVariable);

                    if (symbol.GetEffectiveAccessibility() == Accessibility.Public)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, firstVariable.GetLocation()));
                    }
                },
                SyntaxKind.FieldDeclaration);
        }
    }
}
