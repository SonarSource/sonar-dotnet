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
public sealed class AvoidListForEach : StylingAnalyzer
{
    public AvoidListForEach() : base("T0011", "Use 'foreach' iteration instead of 'List.ForEach'.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var name = ((MemberAccessExpressionSyntax)c.Node).Name;
                if (name.Identifier.ValueText == nameof(List<int>.ForEach)
                    && c.Model.GetSymbolInfo(name).Symbol is IMethodSymbol method
                    && method.Is(KnownType.System_Collections_Generic_List_T, nameof(List<int>.ForEach)))
                {
                    c.ReportIssue(Rule, name);
                }
            },
            SyntaxKind.SimpleMemberAccessExpression);
}
