/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TypeNamesShouldNotMatchNamespaces : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4041";
        private const string MessageFormat = "Change the name of type '{0}' to be different from an existing framework namespace.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        // Based on https://msdn.microsoft.com/en-us/library/gg145045%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
        private static readonly ISet<string> FrameworkNamespaces = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "Accessibility", "Activities", "AddIn", "Build", "CodeDom", "Collections",
                "Componentmodel", "Configuration", "CSharp", "CustomMarshalers", "Data",
                "Dataflow", "Deployment", "Device", "Diagnostics", "DirectoryServices",
                "Drawing", "Dynamic", "EnterpriseServices", "Globalization", "IdentityModel",
                "InteropServices", "IO", "JScript", "Linq", "Location", "Management", "Media",
                "Messaging", "Microsoft", "Net", "Numerics", "Printing", "Reflection", "Resources",
                "Runtime", "Security", "Server", "ServiceModel", "ServiceProcess", "Speech",
                "SqlServer", "System", "Tasks", "Text", "Threading", "Timers", "Transactions",
                "UIAutomationClientsideProviders", "VisualBasic", "VisualC", "Web", "Win32",
                "Windows", "Workflow", "Xaml", "XamlGeneratedNamespace", "Xml"
            };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                if (!c.IsRedundantPositionalRecordContext()
                    && c.Node.GetIdentifier() is { } identifier
                    && FrameworkNamespaces.Contains(identifier.ValueText)
                    && c.Model.GetDeclaredSymbol(c.Node)?.DeclaredAccessibility == Accessibility.Public)
                {
                    c.ReportIssue(Rule, identifier, identifier.ValueText);
                }
            },
            SyntaxKind.ClassDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKindEx.RecordStructDeclaration,
            SyntaxKind.StructDeclaration);
    }
}
