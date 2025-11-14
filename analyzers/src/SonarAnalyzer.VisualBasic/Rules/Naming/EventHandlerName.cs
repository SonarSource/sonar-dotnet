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

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class EventHandlerName : ParametrizedDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2347";
    private const string MessageFormat = "Rename event handler '{0}' to match the regular expression: '{1}'.";
    private const string DefaultPattern = "^(([a-z][a-z0-9]*)?" + NamingPatterns.PascalCasingInternalPattern + "_)?" + NamingPatterns.PascalCasingInternalPattern + "$";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat, false);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    [RuleParameter("format", PropertyType.String, "Regular expression used to check the even handler names against.", DefaultPattern)]
    public string Pattern { get; set; } = DefaultPattern;

    internal static bool IsEventHandler(MethodStatementSyntax declaration, SemanticModel model)
    {
        if (declaration.HandlesClause is not null)
        {
            return true;
        }

        var symbol = model.GetDeclaredSymbol(declaration);
        return symbol is not null && symbol.IsEventHandler();
    }

    protected override void Initialize(SonarParametrizedAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var methodDeclaration = (MethodStatementSyntax)c.Node;
                if (!NamingPatterns.IsRegexMatch(methodDeclaration.Identifier.ValueText, Pattern)
                    && IsEventHandler(methodDeclaration, c.Model))
                {
                    c.ReportIssue(Rule, methodDeclaration.Identifier, methodDeclaration.Identifier.ValueText, Pattern);
                }
            },
            SyntaxKind.SubStatement);
}
