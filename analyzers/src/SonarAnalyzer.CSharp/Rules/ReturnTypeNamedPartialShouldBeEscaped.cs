/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReturnTypeNamedPartialShouldBeEscaped : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S8380";
    private const string MessageFormat = "Return types named 'partial' should be escaped with '@'";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                if (!c.Compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp14)
                    && ResolveReturnType(c.Node) is { } returnType
                    && (returnType.Kind() == SyntaxKindEx.RefType ? ((RefTypeSyntaxWrapper)returnType).Type : returnType) is { } unWrapped
                    && unWrapped is NameSyntax { Arity: 0 } name
                    && name.GetIdentifier() is { Text: "partial" } identifier)
                {
                    c.ReportIssue(Rule, identifier);
                }
            },
            SyntaxKind.MethodDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKindEx.LocalFunctionStatement);

    private static TypeSyntax ResolveReturnType(SyntaxNode node) =>
        node switch
        {
            MethodDeclarationSyntax { Arity: 0, ReturnType: { } x } => x,
            DelegateDeclarationSyntax { Arity: 0, ReturnType: { } x } => x,
            _ when node.IsKind(SyntaxKindEx.LocalFunctionStatement)
                && (LocalFunctionStatementSyntaxWrapper)node is { TypeParameterList: null or { Parameters.Count: 0 }, ReturnType: { } x } => x,
            _ => null,
        };
}
