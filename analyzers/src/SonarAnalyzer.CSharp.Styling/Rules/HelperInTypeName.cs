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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HelperInTypeName : StylingAnalyzer
{
    public HelperInTypeName() : base("T0006", "Do not use 'Helper' in type names.") { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c =>
            {
                if (c.Node.GetIdentifier() is { } identifier && identifier.ValueText.Contains("Helper"))
                {
                    c.ReportIssue(Rule, identifier);
                }
            },
            SyntaxKind.UsingDirective,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.InterfaceDeclaration);

        context.RegisterNodeAction(c =>
            {
                var name = c.Node switch
                {
                    NamespaceDeclarationSyntax ns => ns.Name,
                    FileScopedNamespaceDeclarationSyntax ns => ns.Name
                };
                if (name.ToString().Contains("Helper"))
                {
                    c.ReportIssue(Rule, name);
                }
            },
           SyntaxKind.NamespaceDeclaration,
           SyntaxKindEx.FileScopedNamespaceDeclaration);
    }
}
