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

public abstract class AllParametersBase : StylingAnalyzer
{
    protected abstract void Verify(SonarSyntaxNodeReportingContext context, SyntaxNode[] parameters);

    protected AllParametersBase(string id, string messageFormat) : base(id, messageFormat) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c => Verify(c, Parameters(c.Node).ToArray()),
            SyntaxKind.ParameterList,
            SyntaxKind.BracketedParameterList,
            SyntaxKind.TypeParameterList,
            SyntaxKindEx.FunctionPointerParameterList);

    private static IEnumerable<SyntaxNode> Parameters(SyntaxNode node) =>
        node switch
        {
            FunctionPointerParameterListSyntax list => list.Parameters,
            TypeParameterListSyntax list => list.Parameters,
            BaseParameterListSyntax list => list.Parameters,
            _ => throw new UnexpectedValueException(nameof(node), node?.GetType().ToString())
        };
}
