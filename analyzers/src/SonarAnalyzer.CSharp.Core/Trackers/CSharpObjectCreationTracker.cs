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

namespace SonarAnalyzer.CSharp.Core.Trackers;

public class CSharpObjectCreationTracker : ObjectCreationTracker<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    public override Condition ArgumentAtIndexIsConst(int index) =>
        context => ObjectCreationFactory.Create(context.Node).ArgumentList is { } argumentList
                   && argumentList.Arguments.Count > index
                   && argumentList.Arguments[index].Expression.HasConstantValue(context.Model);

    public override object ConstArgumentForParameter(ObjectCreationContext context, string parameterName) =>
        ObjectCreationFactory.TryCreate(context.Node, out var objectCreation)
        && objectCreation.ArgumentList.ArgumentValuesForParameter(context.Model, parameterName) is { Length: 1 } values
        && values[0] is ExpressionSyntax valueSyntax
            ? valueSyntax.FindConstantValue(context.Model)
            : null;
}
