/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TestClassShouldHaveTestMethod : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2187";
    private const string MessageFormat = "Add some tests to this {0}.";

    private static readonly ImmutableArray<KnownType> HandledSetupAndCleanUpAttributes =
        ImmutableArray.Create(
            // Only applies to MSTest.
            // NUnit has equivalent attributes, but they can only be applied to classes
            // marked with [SetupFixture], which cannot contain tests.
            KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyInitializeAttribute,
            KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyCleanupAttribute);

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var typeDeclaration = (TypeDeclarationSyntax)c.Node;
                if (!c.IsRedundantPositionalRecordContext()
                    && !typeDeclaration.Identifier.IsMissing
                    && c.Model.GetDeclaredSymbol(typeDeclaration) is { } typeSymbol
                    && IsViolatingRule(typeSymbol)
                    && !IsExceptionToTheRule(typeSymbol))
                {
                    c.ReportIssue(Rule, typeDeclaration.Identifier, typeDeclaration.GetDeclarationTypeName());
                }
            },
            SyntaxKind.ClassDeclaration,
            SyntaxKindEx.RecordDeclaration);

    private static bool HasAnyTestMethod(INamespaceOrTypeSymbol symbol) =>
        symbol.GetMembers().OfType<IMethodSymbol>().Any(x => x.IsTestMethod);

    private static bool IsViolatingRule(INamedTypeSymbol symbol) =>
        symbol.IsTestClass
        && !HasAnyTestMethod(symbol);

    private static bool IsExceptionToTheRule(ITypeSymbol symbol) =>
        symbol.IsAbstract
        // Inactive declarations may contain inherited tests, even when the base type is not a test class.
        || symbol.SelfAndBaseTypes.Any(x => HasAnyTestMethod(x) || HasInactiveMemberDeclarations(x))
        || HasSetupOrCleanupAttributes(symbol);

    private static bool HasInactiveMemberDeclarations(ITypeSymbol symbol) =>
        symbol.DeclaringSyntaxReferences.Any(x => x.GetSyntax() is TypeDeclarationSyntax type && ContainsInactiveMemberDeclarations(type));

    private static bool ContainsInactiveMemberDeclarations(TypeDeclarationSyntax type)
    {
        if (!type.ContainsDirectives)
        {
            return false;
        }
        var body = TextSpan.FromBounds(type.OpenBraceToken.Span.End, type.CloseBraceToken.SpanStart);
        var regions = type.ConditionalCompilationRegions()
            .Where(x => body.OverlapsWith(x.Range.Span) && !x.Range.Span.Contains(type.Span))
            .ToList();
        var declarationSpans = DeclarationSpans(type);
        var regionsCuttingDeclarations = regions.Where(x => CutsThroughDeclaration(x, declarationSpans)).ToList();
        return regions.SelectMany(x => x.DisabledSpans).Any(x => !regionsCuttingDeclarations.Any(y => y.Range.Span.Contains(x)));
    }

    private static List<TextSpan> DeclarationSpans(TypeDeclarationSyntax type) =>
        type.Members.Select(DeclarationSpan)
            .Concat(type.DescendantNodes().OfType<AttributeArgumentListSyntax>().Select(x => x.Span))
            .ToList();

    private static TextSpan DeclarationSpan(MemberDeclarationSyntax member)
    {
        var declarationStart = member.ChildNodesAndTokens().FirstOrDefault(x => !x.IsKind(SyntaxKind.AttributeList));
        return declarationStart.IsKind(SyntaxKind.None) ? member.Span : TextSpan.FromBounds(declarationStart.SpanStart, member.Span.End);
    }

    // An active member sharing its closing brace with an inactive alternative extends past the region.
    // Such alternatives are excluded too: spans alone cannot distinguish hidden tests from hidden statements.
    private static bool CutsThroughDeclaration(ConditionalCompilationRegion region, List<TextSpan> declarationSpans) =>
        declarationSpans.Any(x => region.Range.Span.OverlapsPartially(x));

    private static bool HasSetupOrCleanupAttributes(INamespaceOrTypeSymbol symbol) =>
        symbol.GetMembers().OfType<IMethodSymbol>().Any(x => x.GetAttributes(HandledSetupAndCleanUpAttributes).Any());
}
