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

using System;
using System.Collections.Immutable;
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
    public sealed class DoNotThrowFromDestructors : DoNotThrowFromDestructorsBase
    {
        private const string MessageFormat = "Remove this 'Throw' statement.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (IsFinalizer(c.Node.FirstAncestorOrSelf<MethodBlockSyntax>()))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, c.Node.GetLocation()));
                    }
                },
                SyntaxKind.ThrowStatement);
        }

        private bool IsFinalizer(MethodBlockSyntax methodBlockSyntax)
        {
            if (methodBlockSyntax == null)
            {
                return false;
            }

            var subOrFunctionDeclaration = methodBlockSyntax.SubOrFunctionStatement;
            var noParam = subOrFunctionDeclaration.ParameterList == null || subOrFunctionDeclaration.ParameterList.Parameters.Count == 0;
            var noTypeParam = subOrFunctionDeclaration.TypeParameterList == null || subOrFunctionDeclaration.TypeParameterList.Parameters.Count == 0;
            var isSub = subOrFunctionDeclaration.SubOrFunctionKeyword.IsKind(SyntaxKind.SubKeyword);
            var isProtected = subOrFunctionDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ProtectedKeyword));

            return noParam && noTypeParam && isSub && isProtected &&
                subOrFunctionDeclaration.Identifier.ValueText.Equals("Finalize", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
