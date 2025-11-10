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
public sealed class TestMethodShouldContainAssertion : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S2699";
    private const string MessageFormat = "Add at least one assertion to this test case.";
    private const string CustomAssertionAttributeName = "AssertionMethodAttribute";
    private const int MaxInvocationDepth = 2; // Consider BFS instead of DFS if this gets increased

    private static readonly Dictionary<string, KnownType[]> KnownAssertions = new()
    {
        {"DidNotReceive", [KnownType.NSubstitute_SubstituteExtensions] },
        {"DidNotReceiveWithAnyArgs", [KnownType.NSubstitute_SubstituteExtensions] },
        {"Received", [KnownType.NSubstitute_SubstituteExtensions, KnownType.NSubstitute_ReceivedExtensions_ReceivedExtensions] },
        {"ReceivedWithAnyArgs", [KnownType.NSubstitute_SubstituteExtensions, KnownType.NSubstitute_ReceivedExtensions_ReceivedExtensions] },
        {"InOrder", [KnownType.NSubstitute_Received] },
    };

    /// The assertions in the Shouldly, Moq and FsCheck libraries are supported by <see cref="UnitTestHelper.KnownAssertionMethodParts"/>
    /// - All assertions in Shouldly contain "Should" in their name.
    /// - All assertions in Moq contain "Verify" in their name.
    private static readonly ImmutableArray<KnownType> KnownAssertionTypes = ImmutableArray.Create(
        KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert,
        KnownType.NFluent_Check,
        KnownType.NUnit_Framework_Assert,
        KnownType.Xunit_Assert);

    private static readonly ImmutableArray<KnownType> KnownAssertionExceptionTypes = ImmutableArray.Create(
        KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_AssertFailedException,
        KnownType.NFluent_FluentCheckException,
        KnownType.NFluent_Kernel_FluentCheckException,
        KnownType.NUnit_Framework_AssertionException,
        KnownType.Xunit_Sdk_AssertException,
        KnownType.Xunit_Sdk_XunitException);

    private static readonly ImmutableArray<KnownType> FsCheckPropertyAttributes = ImmutableArray.Create(
        KnownType.FsCheck_Xunit_PropertyAttribute,
        KnownType.FsCheck_NUnit_PropertyAttribute);

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var declaration = MethodDeclarationFactory.Create(c.Node);
                if (!declaration.Identifier.IsMissing
                    && declaration.HasImplementation
                    && c.Model.GetDeclaredSymbol(c.Node) is IMethodSymbol method
                    && method.IsTestMethod()
                    && !method.HasExpectedExceptionAttribute()
                    && !method.HasAssertionInAttribute()
                    && !method.IsIgnoredTestMethod()
                    && !method.HasAnyAttribute(FsCheckPropertyAttributes)
                    && !ContainsAssertion(c.Node, c.Model, new HashSet<IMethodSymbol>(), 0))
                {
                    c.ReportIssue(Rule, declaration.Identifier);
                }
            },
            SyntaxKind.MethodDeclaration,
            SyntaxKindEx.LocalFunctionStatement);

    private static bool ContainsAssertion(SyntaxNode methodDeclaration, SemanticModel model, ISet<IMethodSymbol> visitedSymbols, int level)
    {
        var currentModel = methodDeclaration.EnsureCorrectSemanticModelOrDefault(model);
        if (currentModel is null)
        {
            return false;
        }

        var descendantNodes = methodDeclaration.DescendantNodes();
        var invocations = descendantNodes.OfType<InvocationExpressionSyntax>().ToArray();
        if (Array.Exists(invocations, IsAssertion)
            || descendantNodes.OfType<ThrowStatementSyntax>().Any(x => x.Expression is not null && currentModel.GetTypeInfo(x.Expression).Type.DerivesFromAny(KnownAssertionExceptionTypes)))
        {
            return true;
        }

        var invokedSymbols = invocations.Select(x => currentModel.GetSymbolInfo(x).Symbol).OfType<IMethodSymbol>();
        if (invokedSymbols.Any(x => IsKnownAssertion(x) || IsCustomAssertion(x)))
        {
            return true;
        }

        if (level == MaxInvocationDepth)
        {
            return false;
        }

        foreach (var symbol in invokedSymbols.Where(x => !visitedSymbols.Contains(x)))
        {
            visitedSymbols.Add(symbol);
            if (symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).OfType<MethodDeclarationSyntax>().Any(x => ContainsAssertion(x, currentModel, visitedSymbols, level + 1)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAssertion(InvocationExpressionSyntax invocation) =>
        invocation.Expression
            .ToString()
            .SplitCamelCaseToWords()
            .Intersect(KnownMethods.AssertionMethodParts)
            .Any();

    private static bool IsKnownAssertion(ISymbol methodSymbol) =>
        (KnownAssertions.GetValueOrDefault(methodSymbol.Name) is { } types && Array.Exists(types, x => methodSymbol.ContainingType.ConstructedFrom.Is(x)))
        || methodSymbol.ContainingType.DerivesFromAny(KnownAssertionTypes);

    private static bool IsCustomAssertion(ISymbol methodSymbol) =>
        methodSymbol.GetAttributesWithInherited().Any(x => x.AttributeClass.Name == CustomAssertionAttributeName);
}
