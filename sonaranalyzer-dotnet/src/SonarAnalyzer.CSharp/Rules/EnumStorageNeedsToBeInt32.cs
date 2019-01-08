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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class EnumStorageNeedsToBeInt32 : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4022";
        private const string MessageFormat = "Change this enum storage to 'Int32'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var enumDeclaration = (c.Node as EnumDeclarationSyntax);
                    var enumBaseType = enumDeclaration?.BaseList?.Types.FirstOrDefault()?.Type;

                    if (enumDeclaration != null &&
                        !IsInt32OrDefault(enumBaseType, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, enumDeclaration.Identifier.GetLocation()));
                    }
                },
                SyntaxKind.EnumDeclaration);
        }

        private static bool IsInt32OrDefault(SyntaxNode syntaxNode, SemanticModel semanticModel)
        {
            if (syntaxNode == null)
            {
                return true;
            }

            var symbolType = semanticModel.GetSymbolInfo(syntaxNode).Symbol.GetSymbolType();
            return symbolType.Is(KnownType.System_Int32);
        }
    }
}
