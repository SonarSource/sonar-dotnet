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
public sealed class IdentifiersNamedExtensionShouldBeEscaped : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S8368";
    private const string MessageFormat = "'extension' is a contextual keyword in C# 14. Rename it or escape it as '@extension' to avoid ambiguity.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
            {
                if (compilationStart.Compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp14))
                {
                    return;
                }

                compilationStart.RegisterNodeAction(
                    c =>
                    {
                        if (c.Node.GetIdentifier() is { } id && IsExtensionToken(id))
                        {
                            c.ReportIssue(Rule, id);
                        }
                    },
                    SyntaxKind.ClassDeclaration,
                    SyntaxKind.StructDeclaration,
                    SyntaxKind.InterfaceDeclaration,
                    SyntaxKind.EnumDeclaration,
                    SyntaxKind.DelegateDeclaration,
                    SyntaxKind.TypeParameter,
                    SyntaxKind.ConstructorDeclaration,
                    SyntaxKind.UsingDirective,
                    SyntaxKindEx.RecordDeclaration,
                    SyntaxKindEx.RecordStructDeclaration);

                compilationStart.RegisterNodeAction(c =>
                    {
                        if (c.Node.TypeSyntax()?.Unwrap() is { } type and not (QualifiedNameSyntax or AliasQualifiedNameSyntax)
                            && type.GetIdentifier() is { } id
                            && IsExtensionToken(id))
                        {
                            c.ReportIssue(Rule, id);
                        }
                    },
                    SyntaxKind.FieldDeclaration,
                    SyntaxKind.IndexerDeclaration,
                    SyntaxKind.MethodDeclaration,
                    SyntaxKind.OperatorDeclaration,
                    SyntaxKind.PropertyDeclaration);
            });

    private static bool IsExtensionToken(SyntaxToken token) =>
        token.ValueText == "extension" && !token.Text.StartsWith("@", StringComparison.Ordinal);
}
