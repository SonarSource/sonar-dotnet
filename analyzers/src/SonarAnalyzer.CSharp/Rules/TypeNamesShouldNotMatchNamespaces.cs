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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class TypeNamesShouldNotMatchNamespaces : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4041";
        private const string MessageFormat = "Change the name of type '{0}' to be different from an existing framework namespace.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        // Based on https://msdn.microsoft.com/en-us/library/gg145045%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
        private static readonly ISet<string> FrameworkNamespaces =
            new SortedSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "Accessibility", "Activities", "AddIn", "Build", "CodeDom", "Collections",
                "Componentmodel", "Configuration", "CSharp", "CustomMarshalers", "Data",
                "Dataflow", "Deployment", "Device", "Diagnostics", "DirectoryServices",
                "Drawing", "Dynamic", "EnterpriseServices", "Globalization", "IdentityModel",
                "InteropServices", "IO", "JScript", "Linq", "Location", "Management", "Media",
                "Messaging", "Microsoft", "Net", "Numerics", "Printing", "Reflection", "Resources",
                "Runtime", "security", "server", "servicemodel", "serviceprocess", "speech",
                "SqlServer", "System", "Tasks", "Text", "Threading", "Timers", "Transactions",
                "UIAutomationClientsideProviders", "VisualBasic", "VisualC", "Web", "Win32",
                "Windows", "Workflow", "Xaml", "XamlGeneratedNamespace", "Xml"
            };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                if (c.ContainingSymbol.Kind == SymbolKind.NamedType
                    && IsDeclaredPublic(c.Node, c.SemanticModel)
                    && CSharpFacade.Instance.Syntax.NodeIdentifier(c.Node) is { } identifier
                    && FrameworkNamespaces.Contains(identifier.ValueText))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, identifier.GetLocation(), identifier.ValueText));
                }
            },
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKindEx.RecordClassDeclaration);

        private static bool IsDeclaredPublic(SyntaxNode declaration, SemanticModel semanticModel) =>
            semanticModel.GetDeclaredSymbol(declaration)?.DeclaredAccessibility == Accessibility.Public;
    }
}
