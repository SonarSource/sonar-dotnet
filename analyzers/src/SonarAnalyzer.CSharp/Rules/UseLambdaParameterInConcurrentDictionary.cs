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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseLambdaParameterInConcurrentDictionary : UseLambdaParameterInConcurrentDictionaryBase<SyntaxKind, InvocationExpressionSyntax, ArgumentSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override SeparatedSyntaxList<ArgumentSyntax> GetArguments(InvocationExpressionSyntax invocation) =>
         invocation.ArgumentList.Arguments;

    protected override bool IsLambdaAndContainsIdentifier(ArgumentSyntax argument, string keyName) =>
        argument.Expression switch
        {
            SimpleLambdaExpressionSyntax simpleLambda =>
                !simpleLambda.Parameter.GetName().Equals(keyName)
                && IsContainingValidIdentifier(simpleLambda.Body, keyName),
            ParenthesizedLambdaExpressionSyntax parentesizedLambda =>
                !parentesizedLambda.ParameterList.Parameters.Any(x => x.GetName().Equals(keyName))
                && IsContainingValidIdentifier(parentesizedLambda.Body, keyName),
            AnonymousMethodExpressionSyntax anonymousMethod =>
                !anonymousMethod.ParameterList.Parameters.Any(x => x.GetName().Equals(keyName))
                && IsContainingValidIdentifier(anonymousMethod.Block, keyName),
            _ => false
        };

    protected override bool TryGetKeyName(ArgumentSyntax argument, out string keyName)
    {
        keyName = string.Empty;
        if (argument.Expression is IdentifierNameSyntax identifier)
        {
            keyName = identifier.GetName();
            return true;
        }
        return false;
    }

    private bool IsContainingValidIdentifier(SyntaxNode node, string keyName) =>
        node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(p => p.GetName().Equals(keyName) && !IsContainedInNameOfInvocation(p));

    private bool IsContainedInNameOfInvocation(IdentifierNameSyntax identifier) =>
        identifier.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax { Expression: IdentifierNameSyntax expression } } }
        && expression.NameIs("nameof");
}
