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
    public sealed class TypeNamesShouldNotMatchNamespaces : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4041";
        private const string MessageFormat = "Change the name of type '{0}' to be different from an existing framework namespace.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        // Based on https://msdn.microsoft.com/en-us/library/gg145045%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
        private static ISet<string> frameworkNamespaces =
            new HashSet<string>
            {
                "accessibility", "activities", "addin", "build", "codedom", "collections",
                "componentmodel", "configuration", "csharp", "custommarshalers", "data",
                "dataflow", "deployment", "device", "diagnostics", "directoryservices",
                "drawing", "dynamic", "enterpriseservices", "globalization", "identitymodel",
                "interopservices", "io", "jscript", "linq", "location", "management", "media",
                "messaging", "microsoft", "net", "numerics", "printing", "reflection", "resources",
                "runtime", "security", "server", "servicemodel", "serviceprocess", "speech",
                "sqlserver", "system", "tasks", "text", "threading", "timers", "transactions",
                "uiautomationclientsideproviders", "visualbasic", "visualc", "web", "win32",
                "windows", "workflow", "xaml", "xamlgeneratednamespace", "xml"
            };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                if (IsDeclaredPublic(c.Node, c.SemanticModel))
                {
                    ReportIfNameClashesWithFrameworkNamespace(GetIdentifier(c.Node), c);
                }
            },
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.DelegateDeclaration);
        }

        private static SyntaxToken? GetIdentifier(SyntaxNode declaration)
        {
            if (declaration is BaseTypeDeclarationSyntax baseTypeDeclaration)
            {
                return baseTypeDeclaration.Identifier;
            }

            if (declaration is DelegateDeclarationSyntax delegateDeclaration)
            {
                return delegateDeclaration.Identifier;
            }

            return null;
        }

        private static void ReportIfNameClashesWithFrameworkNamespace(SyntaxToken? identifier,
            SyntaxNodeAnalysisContext context)
        {
            var typeName = identifier?.ValueText;
            var typeNameLocation = identifier?.GetLocation();

            var isNameClash = typeName != null &&
                 typeNameLocation != null &&
                 frameworkNamespaces.Contains(typeName.ToLowerInvariant());

            if (isNameClash)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, typeNameLocation, typeName));
            }
        }

        private static bool IsDeclaredPublic(SyntaxNode declaration, SemanticModel semanticModel)
        {
            return semanticModel.GetDeclaredSymbol(declaration)?.DeclaredAccessibility == Accessibility.Public;
        }
    }
}
