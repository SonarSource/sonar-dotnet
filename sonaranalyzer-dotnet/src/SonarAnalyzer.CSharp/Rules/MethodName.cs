/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
    using CamelCaseConverter = ClassName.CamelCaseConverter;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class MethodName : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S100";
        private const string MessageFormat = "Rename {0} '{1}' to match camel case naming rules, {2}.";
        internal const string MessageFormatNonUnderscore = "consider using '{0}'";
        internal const string MessageFormatUnderscore = "trim underscores from the name";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (MethodDeclarationSyntax)c.Node;
                    CheckDeclarationName(declaration, declaration.Identifier, c);
                },
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var declaration = (PropertyDeclarationSyntax)c.Node;
                    CheckDeclarationName(declaration, declaration.Identifier, c);
                },
                SyntaxKind.PropertyDeclaration);
        }

        private static void CheckDeclarationName(MemberDeclarationSyntax member, SyntaxToken identifier, SyntaxNodeAnalysisContext context)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(member);
            if (symbol == null)
            {
                return;
            }

            if (ClassName.IsTypeComRelated(symbol.ContainingType) ||
                symbol.GetInterfaceMember() != null ||
                symbol.GetOverriddenMember() != null ||
                symbol.IsExtern)
            {
                return;
            }

            if (identifier.ValueText.StartsWith("_", StringComparison.Ordinal) ||
                identifier.ValueText.EndsWith("_", StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(rule, identifier.GetLocation(), MethodKindNameMapping[member.Kind()],
                    identifier.ValueText, MessageFormatUnderscore));
                return;
            }

            string suggestion;
            if (TryGetChangedName(identifier.ValueText, out suggestion))
            {
                var messageEnding = string.Format(MessageFormatNonUnderscore, suggestion);
                context.ReportDiagnostic(Diagnostic.Create(rule, identifier.GetLocation(), MethodKindNameMapping[member.Kind()],
                    identifier.ValueText, messageEnding));
            }
        }

        private static bool TryGetChangedName(string identifierName, out string suggestion)
        {
            if (identifierName.Contains('_'))
            {
                suggestion = null;
                return false;
            }

            suggestion = CamelCaseConverter.Convert(identifierName);
            return identifierName != suggestion;
        }

        private static readonly Dictionary<SyntaxKind, string> MethodKindNameMapping = new Dictionary<SyntaxKind, string>
        {
            {SyntaxKind.MethodDeclaration, "method" },
            {SyntaxKind.PropertyDeclaration, "property" }
        };
    }
}
