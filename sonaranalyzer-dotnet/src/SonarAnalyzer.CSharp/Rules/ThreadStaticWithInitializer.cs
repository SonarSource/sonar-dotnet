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
    public sealed class ThreadStaticWithInitializer : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2996";
        private const string MessageFormat = "Remove this initialization of '{0}' or make it lazy.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax)c.Node;

                    if (!fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                        !HasThreadStaticAttribute(fieldDeclaration.AttributeLists, c.SemanticModel))
                    {
                        return;
                    }

                    foreach (var variableDeclaratorSyntax in fieldDeclaration.Declaration.Variables
                        .Where(variableDeclaratorSyntax => variableDeclaratorSyntax.Initializer != null))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, variableDeclaratorSyntax.Initializer.GetLocation(),
                            variableDeclaratorSyntax.Identifier.ValueText));
                    }
                },
                SyntaxKind.FieldDeclaration);
        }
        private static bool HasThreadStaticAttribute(SyntaxList<AttributeListSyntax> attributeLists, SemanticModel semanticModel)
        {
            if (!attributeLists.Any())
            {
                return false;
            }

            return attributeLists.Any(attributeList =>
                attributeList.Attributes.Any(attribute => semanticModel.GetTypeInfo(attribute).Type.Is(KnownType.System_ThreadStaticAttribute)));
        }
    }
}
