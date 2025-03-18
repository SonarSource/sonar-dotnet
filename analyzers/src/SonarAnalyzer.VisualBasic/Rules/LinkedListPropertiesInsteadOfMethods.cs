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

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class LinkedListPropertiesInsteadOfMethods : LinkedListPropertiesInsteadOfMethodsBase<SyntaxKind, InvocationExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override bool IsRelevantCallAndType(InvocationExpressionSyntax invocation, SemanticModel model) =>
        invocation.Operands().Right is { } right
        && IsRelevantType(right, model)
        && IsCorrectType(invocation, model);

    private static bool IsCorrectType(InvocationExpressionSyntax invocation, SemanticModel model) =>
        invocation?.ArgumentList?.Arguments is { Count: 1 } args
        && model.GetTypeInfo(args[0].GetExpression()).Type is { } type
        && type.DerivesFrom(KnownType.System_Collections_Generic_LinkedList_T);
}
