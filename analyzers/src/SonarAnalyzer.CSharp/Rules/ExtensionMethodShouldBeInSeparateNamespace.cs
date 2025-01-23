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
    public sealed class ExtensionMethodShouldBeInSeparateNamespace : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4226";
        private const string MessageFormat = "Either move this extension to another namespace or move the method inside the class itself.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;

                    if (methodDeclaration.IsExtensionMethod()
                        && c.Model.GetDeclaredSymbol(methodDeclaration) is { IsExtensionMethod: true, Parameters: { Length: > 0 } } methodSymbol
                        && methodSymbol.Parameters[0].Type.Kind != SymbolKind.ErrorType
                        && methodSymbol.Parameters[0].Type.IsClass()
                        && methodSymbol.ContainingNamespace.Equals(methodSymbol.Parameters[0].Type.ContainingNamespace)
                        && !methodSymbol.Parameters[0].Type.HasAttribute(KnownType.System_CodeDom_Compiler_GeneratedCodeAttribute))
                    {
                        c.ReportIssue(Rule, methodDeclaration.Identifier);
                    }
                },
                SyntaxKind.MethodDeclaration);
    }
}
