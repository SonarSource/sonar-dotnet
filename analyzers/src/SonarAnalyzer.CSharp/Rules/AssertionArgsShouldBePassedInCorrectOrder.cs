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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AssertionArgsShouldBePassedInCorrectOrder : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S3415";
    private const string MessageFormat = "Make sure these 2 arguments are in the correct order: expected value, actual value.";

    private const string Expected = "expected";
    private const string Actual = "actual";
    private const string NotExpected = "notExpected";
    private const string ExpectedSubstring = "expectedSubstring";
    private const string ActualString = "actualString";
    private const string ExpectedEndString = "expectedEndString";
    private const string ExpectedStartString = "expectedStartString";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly KnownAssertParameters[] NUnitParameters =
    [
        new(KnownType.NUnit_Framework_Assert, Expected, Actual),
        new(KnownType.NUnit_Framework_Legacy_ClassicAssert, Expected, Actual)
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                if (c.Node is InvocationExpressionSyntax { ArgumentList.Arguments.Count: >= 2 } invocation
                    && Parameters(invocation.GetName()) is { } knownAssertParameters
                    && c.Model.GetSymbolInfo(invocation).AllSymbols()
                        .SelectMany(symbol =>
                            symbol is IMethodSymbol { IsStatic: true, ContainingSymbol: INamedTypeSymbol container } methodSymbol
                                ? knownAssertParameters.Select(knownParameters => FindWrongArguments(c.Model, container, methodSymbol, invocation, knownParameters))
                                : [])
                        .FirstOrDefault(x => x is not null) is { ExpectedArgs: { } wrongExpected, ActualArgs: { } wrongActual })
                {
                    c.ReportIssue(Rule, CreateLocation(wrongExpected, wrongActual));
                }
            },
            SyntaxKind.InvocationExpression);

    private static KnownAssertParameters[] Parameters(string name) =>
        name switch
        {
            "AreEqual" =>
            [
                new(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, Expected, Actual),
                ..NUnitParameters
            ],
            "AreNotEqual" =>
            [
                new(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, NotExpected, Actual),
                ..NUnitParameters
            ],
            "AreSame" =>
            [
                new(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, Expected, Actual),
                ..NUnitParameters
            ],
            "AreNotSame" =>
            [
                new(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, NotExpected, Actual),
                ..NUnitParameters
            ],
            "Equal" or "NotEqual" or "Same" or "NotSame" or "Equivalent" or "EquivalentWithExclusions" or "StrictEqual" or "NotStrictEqual" =>
            [
                new(KnownType.Xunit_Assert, Expected, Actual)
            ],
            "Contains" or "DoesNotContain" =>
            [
                new(KnownType.Xunit_Assert, ExpectedSubstring, ActualString),
            ],
            "EndsWith" =>
            [
                new(KnownType.Xunit_Assert, ExpectedEndString, ActualString)
            ],
            "StartsWith" =>
            [
                new(KnownType.Xunit_Assert, ExpectedStartString, ActualString)
            ],
            _ => null
        };

    private static WrongArguments? FindWrongArguments(SemanticModel model,
                                                      INamedTypeSymbol container,
                                                      IMethodSymbol symbol,
                                                      InvocationExpressionSyntax invocation,
                                                      KnownAssertParameters knownParameters) =>
        container.Is(knownParameters.AssertClass)
        && CSharpFacade.Instance.MethodParameterLookup(invocation, symbol) is var parameterLookup
        && parameterLookup.TryGetSyntax(knownParameters.ExpectedParameterName, out var expectedArguments)
        && expectedArguments.FirstOrDefault() is { } expected
        && !model.GetConstantValue(expected).HasValue
        && parameterLookup.TryGetSyntax(knownParameters.ActualParameterName, out var actualArguments)
        && actualArguments.FirstOrDefault() is { } actual
        && model.GetConstantValue(actual).HasValue
            ? new(expected, actual)
            : null;

    private static Location CreateLocation(SyntaxNode argument1, SyntaxNode argument2) =>
        argument1.Span.CompareTo(argument2.Span) < 0
            ? argument1.Parent.CreateLocation(argument2.Parent)
            : argument2.Parent.CreateLocation(argument1.Parent);

    private readonly record struct KnownAssertParameters(KnownType AssertClass, string ExpectedParameterName, string ActualParameterName);

    private readonly record struct WrongArguments(SyntaxNode ExpectedArgs, SyntaxNode ActualArgs);
}
