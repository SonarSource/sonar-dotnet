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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SpecifyIFormatProviderOrCultureInfo : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S4056";
    private const string MessageFormat = "Use the overload that takes a 'CultureInfo' or 'IFormatProvider' parameter.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ImmutableArray<KnownType> FormatAndCultureType =
        ImmutableArray.Create(
            KnownType.System_IFormatProvider,
            KnownType.System_Globalization_CultureInfo);

    private static readonly ImmutableArray<KnownType> FormattableTypes =
        ImmutableArray.Create(
            KnownType.System_String,
            KnownType.System_Object);

    private static readonly IReadOnlyCollection<MemberDescriptor> IgnoredMethods =
    [
        new(KnownType.System_Activator, "CreateInstance"),
        new(KnownType.System_Resources_ResourceManager, "GetObject"),
        new(KnownType.System_Resources_ResourceManager, "GetString"),
        // Those methods take a IFormatProvider parameter, but it is not used in the implementation.
        // https://github.com/dotnet/runtime/blob/c0c7f02be7285a1ef0d3022b8c2f38be4025545f/src/libraries/System.Private.CoreLib/src/System/Guid.cs#L1825
        new(KnownType.System_Guid, "Parse"),
        // https://github.com/dotnet/runtime/blob/c0c7f02be7285a1ef0d3022b8c2f38be4025545f/src/libraries/System.Private.CoreLib/src/System/Guid.cs#L1838
        new(KnownType.System_Guid, "TryParse"),
        // These methods take a IFormatProvider parameter and use it.
        // Controversial: There are edge cases like culture "fa-AF" where the 0x2212 (MINUS SIGN) is used instead of the usual 0x002D (HYPHEN-MINUS)
        new(KnownType.System_Int16, "Parse"),
        new(KnownType.System_Int16, "TryParse"),
        new(KnownType.System_Int32, "Parse"),
        new(KnownType.System_Int32, "TryParse"),
        new(KnownType.System_Int64, "Parse"),
        new(KnownType.System_Int64, "TryParse"),
        new(KnownType.System_Int128, "Parse"),
        new(KnownType.System_Int128, "TryParse"),
    ];

    private static IReadOnlyCollection<MemberDescriptor> WhitelistedMethods =
    [
        new(KnownType.System_Char, "ToUpper"),
        new(KnownType.System_Char, "ToLower"),
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    public static bool HasAnyFormatOrCultureParameter(ISymbol method) =>
        method.GetParameters().Any(x => x.Type.IsAny(FormatAndCultureType));

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var invocation = (InvocationExpressionSyntax)c.Node;
                if (invocation.Expression is not null
                    && c.Model.GetSymbolInfo(invocation.Expression).Symbol is IMethodSymbol methodSymbol
                    && !IsIgnored(methodSymbol)
                    && CanPotentiallyRaise(methodSymbol)
                    && invocation.HasOverloadWithType(c.Model, FormatAndCultureType))
                {
                    c.ReportIssue(Rule, invocation, invocation.Expression.ToString());
                }
            },
            SyntaxKind.InvocationExpression);

    private static bool CanPotentiallyRaise(IMethodSymbol methodSymbol) =>
        ReturnsOrAcceptsFormattableType(methodSymbol)
        || WhitelistedMethods.Any(x => Matches(x, methodSymbol));

    private static bool IsIgnored(IMethodSymbol methodSymbol) =>
        SpecifyStringComparison.HasAnyStringComparisonParameter(methodSymbol)
        || HasAnyFormatOrCultureParameter(methodSymbol)
        || IgnoredMethods.Any(x => Matches(x, methodSymbol));

    private static bool ReturnsOrAcceptsFormattableType(IMethodSymbol methodSymbol) =>
        methodSymbol.ReturnType.IsAny(FormattableTypes)
        || methodSymbol.GetParameters().Any(x => x.Type.IsAny(FormattableTypes));

    private static bool Matches(MemberDescriptor memberDescriptor, IMethodSymbol methodSymbol) =>
        methodSymbol is not null
        && methodSymbol.ContainingType.Is(memberDescriptor.ContainingType)
        && methodSymbol.Name == memberDescriptor.Name;
}
