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
public sealed class AvoidGet : StylingAnalyzer
{
    public AvoidGet() : base("T0000", "Do not use 'Get' prefix. Just don't.") { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c => Process(c, ((MethodDeclarationSyntax)c.Node).Identifier), SyntaxKind.MethodDeclaration);
        context.RegisterNodeAction(c => Process(c, ((LocalFunctionStatementSyntax)c.Node).Identifier), SyntaxKind.LocalFunctionStatement);
    }

    private void Process(SonarSyntaxNodeReportingContext context, SyntaxToken identifier)
    {
        if (HasGetPrefix(identifier.ValueText) && !IgnoreMethod((IMethodSymbol)context.ContainingSymbol))
        {
            context.ReportIssue(Rule, identifier);
        }
    }

    private static bool HasGetPrefix(string name) =>
        name.Length > 4 && name.StartsWith("Get") && !char.IsLower(name[3]);

    private static bool IgnoreMethod(IMethodSymbol method) =>
        method.ReturnsVoid
        || method.IsOverride
        || method.ExplicitOrImplicitInterfaceImplementations().Any();
}
