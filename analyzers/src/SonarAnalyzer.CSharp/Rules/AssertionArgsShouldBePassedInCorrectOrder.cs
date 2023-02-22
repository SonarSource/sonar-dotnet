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

using System.Linq.Expressions;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AssertionArgsShouldBePassedInCorrectOrder : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S3415";
    private const string MessageFormat = "Make sure these 2 arguments are in the correct order: expected value, actual value.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    private static readonly IDictionary<string, ImmutableArray<KnownType>> MethodsWithType = new Dictionary<string, ImmutableArray<KnownType>>
    {
        ["AreEqual"]    = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
        ["AreNotEqual"] = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
        ["AreSame"]     = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
        ["AreNotSame"]  = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
        ["Equal"]       = ImmutableArray.Create(KnownType.Xunit_Assert),
        ["Same"]        = ImmutableArray.Create(KnownType.Xunit_Assert),
        ["NotSame"]     = ImmutableArray.Create(KnownType.Xunit_Assert)
    };

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
        {
            if (c.Node is InvocationExpressionSyntax { ArgumentList: { Arguments: { Count: >= 2 } arguments } argumentList } invocation
                && CSharpFacade.Instance.MethodParameterLookup(argumentList, c.SemanticModel) is var parameterLookup
                && CheckArguments(parameterLookup)
                && AssertionMethods(invocation, c.SemanticModel))
            {
                c.ReportIssue(Diagnostic.Create(Rule, arguments[0].CreateLocation(arguments[1])));
            }
        },
        SyntaxKind.InvocationExpression);

    private static bool AssertionMethods(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
        invocation.GetName() switch
        {
            "AreEqual" or "AreNotEqual" => Check(invocation, semanticModel, KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
            "AreSame" or "AreNotSame" => Check(invocation, semanticModel, KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
            "Equal" or "Same" or "NotSame" => Check(invocation, semanticModel, KnownType.Xunit_Assert),
            _ => false
        };

    private static bool Check(InvocationExpressionSyntax invocation, SemanticModel semanticModel, params KnownType[] knownTypes) =>
        semanticModel.GetSymbolInfo(invocation).AllSymbols().All(x =>
            x is IMethodSymbol { IsStatic: true, ContainingSymbol: INamedTypeSymbol container }
            && container.IsAny(knownTypes));

    private static bool CheckArguments(IMethodParameterLookup parameterLookup) =>
        // "notExpected" is used in MSTest's AreNotEqual and AreNotSame
        (parameterLookup.TryGetSyntax("expected", out var expected) || parameterLookup.TryGetSyntax("notExpected", out expected))
        && expected.FirstOrDefault() is not LiteralExpressionSyntax
        && parameterLookup.TryGetSyntax("actual", out var actual)
        && actual.FirstOrDefault() is LiteralExpressionSyntax;
}
