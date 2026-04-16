/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
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
public sealed class IdentifiersNamedFieldShouldBeEscaped : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S8367";
    private const string MessageFormat = "'field' is a contextual keyword in C# 14. Rename it, escape it as '@field', or qualify member access as 'this.field' to avoid ambiguity.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
            {
                if (!compilationStart.Compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp14))
                {
                    compilationStart.RegisterCodeBlockStartAction<SyntaxKind>(CSharpGeneratedCodeRecognizer.Instance, cb =>
                        {
                            if ((cb.CodeBlock is AccessorDeclarationSyntax { Parent.Parent: PropertyDeclarationSyntax } accessor
                                    && accessor.Kind() is SyntaxKind.GetAccessorDeclaration or SyntaxKind.SetAccessorDeclaration or SyntaxKindEx.InitAccessorDeclaration)
                                || cb.CodeBlock is ArrowExpressionClauseSyntax { Parent: PropertyDeclarationSyntax })
                            {
                                RegisterForPropertyBody(cb);
                            }
                        });
                }
            });

    private static void RegisterForPropertyBody(SonarCodeBlockStartAnalysisContext<SyntaxKind> cb)
    {
        // Report local variable declarations, local functions, loop variables, catch variables, LINQ range variables, pattern/deconstruction variables, and parameters named 'field'.
        cb.RegisterNodeAction(
            c =>
            {
                if (c.Node.GetIdentifier() is { } identifier && IsFieldToken(identifier))
                {
                    c.ReportIssue(Rule, identifier);
                }
            },
            SyntaxKind.VariableDeclarator,
            SyntaxKind.ForEachStatement,
            SyntaxKind.CatchDeclaration,
            SyntaxKind.FromClause,
            SyntaxKind.LetClause,
            SyntaxKind.JoinClause,
            SyntaxKind.JoinIntoClause,
            SyntaxKind.QueryContinuation,
            SyntaxKindEx.SingleVariableDesignation,
            SyntaxKindEx.LocalFunctionStatement,
            SyntaxKind.Parameter);

        // Report unqualified references to a named symbol 'field' (class members, types, namespaces, …).
        // Exclude locally-declared symbols (locals, parameters, range variables) — reported at their declaration site above.
        cb.RegisterNodeAction(
            c =>
            {
                if (c.Node is IdentifierNameSyntax { Identifier: var identifier, Parent: { } parent } identifierName
                    && IsFieldToken(identifier)
                    && parent is not MemberBindingExpressionSyntax
                    && !(parent is MemberAccessExpressionSyntax mae && mae.Name == identifierName)
                    && c.Model.GetSymbolInfo(identifierName).Symbol is not (null or ILocalSymbol or IParameterSymbol or IRangeVariableSymbol))
                {
                    c.ReportIssue(Rule, identifier);
                }
            },
            SyntaxKind.IdentifierName);
    }

    private static bool IsFieldToken(SyntaxToken token) =>
        token.ValueText == "field" && !token.Text.StartsWith("@", StringComparison.Ordinal);
}
