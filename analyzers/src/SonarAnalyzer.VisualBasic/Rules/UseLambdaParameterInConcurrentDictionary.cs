/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.Rules.VisualBasic;

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
