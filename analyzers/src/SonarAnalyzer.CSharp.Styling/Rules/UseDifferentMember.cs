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
public class UseDifferentMember : StylingAnalyzer
{
    public UseDifferentMember() : base("T0047", "Use '{0}' instead of '{1}'.{2}") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var identifier = (IdentifierNameSyntax)c.Node;
                if (identifier.Identifier.Text is nameof(IMethodSymbol.IsExtensionMethod)
                    && c.Model.GetSymbolInfo(identifier) is { Symbol: IPropertySymbol property }
                    && property.IsInType(KnownType.Microsoft_CodeAnalysis_IMethodSymbol))
                {
                    c.ReportIssue(Rule, c.Node, "IsExtension", nameof(IMethodSymbol.IsExtensionMethod), " It also covers extension methods defined in extension blocks.");
                }
            },
            SyntaxKind.IdentifierName);
}
