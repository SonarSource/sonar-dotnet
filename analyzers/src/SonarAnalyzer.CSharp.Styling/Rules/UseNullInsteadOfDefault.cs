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
public sealed class UseNullInsteadOfDefault : StylingAnalyzer
{
    public UseNullInsteadOfDefault() : base("T0012", "Use 'null' instead of 'default' for reference types.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                if (IsReferenceType(c.Node, c.Model))
                {
                    c.ReportIssue(Rule, c.Node);
                }
            },
            SyntaxKind.DefaultLiteralExpression, SyntaxKind.DefaultExpression);

    private static bool IsReferenceType(SyntaxNode node, SemanticModel model)
    {
        var type = model.GetTypeInfo(node).Type;
        return (type.IsReferenceType && type is not IErrorTypeSymbol) || type.Is(KnownType.System_Nullable_T);
    }
}
