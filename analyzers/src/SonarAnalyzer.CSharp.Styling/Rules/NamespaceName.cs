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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NamespaceName : StylingAnalyzer
{
    public NamespaceName() : base("T0037", "Use {0} namespace.", SourceScope.Tests) { }     // IDE0030 is aligning namespace with folder for most of the cases

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var declaration = (FileScopedNamespaceDeclarationSyntax)c.Node;
                var removableUsing = declaration.Name.ToString().Replace(".Test.", ".");
                if (c.Tree.GetRoot().ChildNodes().OfType<UsingDirectiveSyntax>().Any(x => x.Name.ToString() == removableUsing))
                {
                    c.ReportIssue(Rule, c.Node, removableUsing + ".Test");
                }
            },
            SyntaxKind.FileScopedNamespaceDeclaration);
}
