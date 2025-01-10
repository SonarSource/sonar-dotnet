/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EmptyNamespace : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3261";
        private const string MessageFormat = "Remove this empty namespace.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var namespaceDeclaration = (BaseNamespaceDeclarationSyntaxWrapper)c.Node;
                    if (!namespaceDeclaration.Members.Any())
                    {
                        c.ReportIssue(Rule, namespaceDeclaration.SyntaxNode);
                    }
                },
                SyntaxKind.NamespaceDeclaration,
                SyntaxKindEx.FileScopedNamespaceDeclaration);
    }
}
