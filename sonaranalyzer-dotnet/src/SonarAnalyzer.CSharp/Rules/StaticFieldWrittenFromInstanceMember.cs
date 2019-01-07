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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class StaticFieldWrittenFromInstanceMember : StaticFieldWrittenFrom
    {
        internal const string DiagnosticId = "S2696";
        private const string MessageFormat = "{0}";
        internal const string MessageFormatMultipleOptions = "Make the enclosing instance {0} 'static' or remove this set on the 'static' field.";
        internal const string MessageFormatRemoveSet = "Remove this set, which updates a 'static' field from an instance {0}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override bool IsValidCodeBlockContext(SyntaxNode node, ISymbol owningSymbol)
        {
            if (owningSymbol == null || owningSymbol.IsStatic)
            {
                return false;
            }

            SyntaxNode declaration = node as MethodDeclarationSyntax;
            if (declaration == null)
            {
                declaration = node as AccessorDeclarationSyntax;
                if (declaration == null)
                {
                    return false;
                }
            }

            return true;
        }

        protected override string GetDiagnosticMessageArgument(SyntaxNode node, ISymbol owningSymbol, IFieldSymbol field)
        {
            var messageFormat = owningSymbol.IsChangeable()
                               ? MessageFormatMultipleOptions
                               : MessageFormatRemoveSet;
            var declarationType = GetDeclarationType(node);

            return string.Format(messageFormat, declarationType);
        }

        private static string GetDeclarationType(SyntaxNode declaration)
        {
            if (declaration is MethodDeclarationSyntax)
            {
                return "method";
            }

            if (declaration is AccessorDeclarationSyntax)
            {
                return "property";
            }

            return string.Empty;
        }
    }
}
