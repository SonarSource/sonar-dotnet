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
public sealed class AllParametersOnSameLine : StylingAnalyzer
{
    public AllParametersOnSameLine() : base("T0023", "Parameters should be on the same line or all on separate lines.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                Verify,
                SyntaxKind.ParameterList,
                SyntaxKind.BracketedParameterList,
                SyntaxKind.TypeParameterList,
                SyntaxKindEx.FunctionPointerParameterList);

    private void Verify(SonarSyntaxNodeReportingContext context)
    {
        var identifiers = GetParameterIdentifiers(context.Node);
        if (identifiers.Count < 3)
        {
            return;
        }

        var isSameLine = IsSameLine(identifiers[0], identifiers[1]);
        for (var i = 2; i < identifiers.Count; i++)
        {
            if (isSameLine != IsSameLine(identifiers[i], identifiers[i - 1]))
            {
                var location = identifiers[i].IsToken ? identifiers[i].Parent.GetLocation() : identifiers[i].GetLocation();
                context.ReportIssue(Rule, location);
                return;
            }
        }
    }

    private static bool IsSameLine(SyntaxNodeOrToken first, SyntaxNodeOrToken second) =>
        first.GetLocation().StartLine() == second.GetLocation().StartLine();

    private static List<SyntaxNodeOrToken> GetParameterIdentifiers(SyntaxNode node) =>
        node.Kind() switch
        {
            SyntaxKindEx.FunctionPointerParameterList => node.DescendantNodesAndTokens().Where(x => x.IsKind(SyntaxKindEx.FunctionPointerParameter)).ToList(),
            SyntaxKind.TypeParameterList => node.DescendantNodesAndTokens().Where(x => x.IsKind(SyntaxKind.TypeParameter)).ToList(),
            _ => node.DescendantNodes().Where(x => x.IsKind(SyntaxKind.Parameter)).Select(x => x.DescendantNodesAndTokens().LastOrDefault(y => y.IsKind(SyntaxKind.IdentifierToken))).ToList()
        };
}
