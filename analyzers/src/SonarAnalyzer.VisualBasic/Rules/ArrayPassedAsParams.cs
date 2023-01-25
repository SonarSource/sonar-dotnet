/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
public sealed class ArrayPassedAsParams : ArrayPassedAsParamsBase<SyntaxKind, InvocationExpressionSyntax, ObjectCreationExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;
    protected override string ParameterKeyword => "ParamArray";
    protected override string MessageFormat => MessageBase;

    protected override bool ShouldReportInvocation(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation)
    {
        var myLookup = new VisualBasicMethodParameterLookup(invocation, context.SemanticModel);

        return invocation.ArgumentList.Arguments.Count > 0
        && invocation.ArgumentList.Arguments.Last().GetExpression() is ArrayCreationExpressionSyntax array
        && CheckArrayInitializer(array)
        && myLookup.TryGetSymbol(invocation.ArgumentList.Arguments.Last(), out var parameter)
        && parameter.IsParams;
    }
    protected override bool ShouldReportCreation(ObjectCreationExpressionSyntax creation) =>
        creation.ArgumentList is not null
        && creation.ArgumentList.Arguments.Count > 0
        && creation.ArgumentList.Arguments.Last().GetExpression() is ArrayCreationExpressionSyntax array
        && CheckArrayInitializer(array);

    protected override Location GetInvocationLocation(InvocationExpressionSyntax invocation) =>
        invocation.ArgumentList.Arguments.Last().GetExpression().GetLocation();

    protected override Location GetCreationLocation(ObjectCreationExpressionSyntax creation) =>
        creation.ArgumentList.Arguments.Last().GetExpression().GetLocation();

    private static bool CheckArrayInitializer(ArrayCreationExpressionSyntax array) =>
        array.Initializer is CollectionInitializerSyntax initializer
        && initializer.Initializers.Count > 0;
}
