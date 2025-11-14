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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.VisualBasic.Core.Trackers;

public class VisualBasicObjectCreationTracker : ObjectCreationTracker<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    public override Condition ArgumentAtIndexIsConst(int index) =>
        context => ((ObjectCreationExpressionSyntax)context.Node).ArgumentList  is { } argumentList
                   && argumentList.Arguments.Count > index
                   && argumentList.Arguments[index].GetExpression().HasConstantValue(context.Model);

    public override object ConstArgumentForParameter(ObjectCreationContext context, string parameterName) =>
        ((ObjectCreationExpressionSyntax)context.Node).ArgumentList is { } argumentList
            && argumentList.ArgumentValuesForParameter(context.Model, parameterName) is { Length: 1 } values
            && values[0] is ExpressionSyntax valueSyntax
                ? valueSyntax.FindConstantValue(context.Model)
                : null;
}
