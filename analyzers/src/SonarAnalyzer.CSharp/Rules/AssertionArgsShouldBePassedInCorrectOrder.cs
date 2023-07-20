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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AssertionArgsShouldBePassedInCorrectOrder : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S3415";
    private const string MessageFormat = "Make sure these 2 arguments are in the correct order: expected value, actual value.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
        {
            if (c.Node is InvocationExpressionSyntax { ArgumentList: { Arguments.Count: >= 2 } argumentList } invocation
                && GetParameters(invocation.GetName()) is { } knownAssertParameters
                && c.SemanticModel.GetSymbolInfo(invocation).AllSymbols()
                    .SelectMany(symbol =>
                        symbol is IMethodSymbol { IsStatic: true, ContainingSymbol: INamedTypeSymbol container } methodSymbol
                            ? knownAssertParameters.Select(knownParameters => FindWrongArguments(c.SemanticModel, container, methodSymbol, argumentList, knownParameters))
                            : Enumerable.Empty<WrongArguments?>())
                    .FirstOrDefault(x => x is not null) is (Expected: var expected, Actual: var actual))
            {
                c.ReportIssue(CreateDiagnostic(Rule, CreateLocation(expected, actual)));
            }
        },
        SyntaxKind.InvocationExpression);

    private static KnownAssertParameters[] GetParameters(string name) =>
        name switch
        {
            "AreEqual" => new KnownAssertParameters[]
                {
                    new(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, "expected", "actual"),
                    new(KnownType.NUnit_Framework_Assert, "expected", "actual")
                },
            "AreNotEqual" => new KnownAssertParameters[]
                {
                    new(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, "notExpected", "actual"),
                    new(KnownType.NUnit_Framework_Assert, "expected", "actual")
                },
            "AreSame" => new KnownAssertParameters[]
                {
                    new(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, "expected", "actual"),
                    new(KnownType.NUnit_Framework_Assert, "expected", "actual")
                },
            "AreNotSame" => new KnownAssertParameters[]
                {
                    new(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, "notExpected", "actual"),
                    new(KnownType.NUnit_Framework_Assert, "expected", "actual")
                },
            "Equal" or "NotEqual" or "Same" or "NotSame" => new KnownAssertParameters[]
                {
                    new(KnownType.Xunit_Assert, "expected", "actual")
                },
            _ => null
        };

    private static WrongArguments? FindWrongArguments(SemanticModel semanticModel,
                                                      INamedTypeSymbol container,
                                                      IMethodSymbol symbol,
                                                      ArgumentListSyntax argumentList,
                                                      KnownAssertParameters knownParameters) =>
        container.Is(knownParameters.AssertClass)
        && CSharpFacade.Instance.MethodParameterLookup(argumentList, symbol) is var parameterLookup
        && parameterLookup.TryGetSyntax(knownParameters.ExpectedParameterName, out var expectedArguments)
        && expectedArguments.FirstOrDefault() is { } expected
        && semanticModel.GetConstantValue(expected).HasValue is false
        && parameterLookup.TryGetSyntax(knownParameters.ActualParameterName, out var actualArguments)
        && actualArguments.FirstOrDefault() is { } actual
        && semanticModel.GetConstantValue(actual).HasValue
            ? new(expected, actual)
            : null;

    private static Location CreateLocation(SyntaxNode argument1, SyntaxNode argument2) =>
        argument1.Span.CompareTo(argument2.Span) < 0
            ? argument1.Parent.CreateLocation(argument2.Parent)
            : argument2.Parent.CreateLocation(argument1.Parent);

    private readonly record struct KnownAssertParameters(KnownType AssertClass, string ExpectedParameterName, string ActualParameterName);
    private readonly record struct WrongArguments(SyntaxNode Expected, SyntaxNode Actual);
}
