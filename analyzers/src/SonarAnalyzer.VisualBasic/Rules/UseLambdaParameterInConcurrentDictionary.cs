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
public sealed class UseLambdaParameterInConcurrentDictionary : UseLambdaParameterInConcurrentDictionaryBase<SyntaxKind, InvocationExpressionSyntax, ArgumentSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override SeparatedSyntaxList<ArgumentSyntax> GetArguments(InvocationExpressionSyntax invocation) =>
         invocation.ArgumentList.Arguments;

    protected override bool IsLambdaAndContainsIdentifier(ArgumentSyntax argument, string keyName) =>
        argument.GetExpression() is SingleLineLambdaExpressionSyntax lambda
        && lambda.Body.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(p => p.GetName().Equals(keyName) && p.Parent is not NameOfExpressionSyntax);

    protected override bool TryGetKeyName(ArgumentSyntax argument, out string keyName)
    {
        keyName = string.Empty;
        if (argument.GetExpression() is IdentifierNameSyntax identifier)
        {
            keyName = identifier.GetName();
            return true;
        }
        return false;
    }
}
