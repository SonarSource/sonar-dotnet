/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SpecifyIFormatProviderOrCultureInfo : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4056";
        private const string MessageFormat = "Use the overload that takes a 'CultureInfo' or 'IFormatProvider' parameter.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> formatAndCultureType =
            ImmutableArray.Create(
                KnownType.System_IFormatProvider,
                KnownType.System_Globalization_CultureInfo
            );

        private static readonly ImmutableArray<KnownType> formattableTypes =
            ImmutableArray.Create(
                KnownType.System_String,
                KnownType.System_Object
            );

        private static readonly ISet<MemberDescriptor> whitelistMethods = new HashSet<MemberDescriptor>
        {
            new MemberDescriptor(KnownType.System_Activator, "CreateInstance"),
            new MemberDescriptor(KnownType.System_Resources_ResourceManager, "GetObject"),
            new MemberDescriptor(KnownType.System_Resources_ResourceManager, "GetString"),
        };

        private static readonly ISet<MemberDescriptor> blacklistMethods = new HashSet<MemberDescriptor>
        {
            new MemberDescriptor(KnownType.System_Char, "ToUpper"),
            new MemberDescriptor(KnownType.System_Char, "ToLower"),
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    if (invocation.Expression != null &&
                        c.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol is IMethodSymbol methodSymbol &&
                        !IsIgnored(methodSymbol) &&
                        CanPotentiallyRaise(methodSymbol) &&
                        SonarAnalyzer.CSharp.Syntax.Extensions.InvocationExpressionSyntaxExtensions.HasOverloadWithType(invocation, c.SemanticModel, formatAndCultureType))
                    {
                        c.ReportIssue(rule, invocation, invocation.Expression.ToString());
                    }
                }, SyntaxKind.InvocationExpression);
        }

        private static bool IsIgnored(IMethodSymbol methodSymbol) =>
            SpecifyStringComparison.HasAnyStringComparisonParameter(methodSymbol) ||
            HasAnyFormatOrCultureParameter(methodSymbol) ||
            whitelistMethods.Any(x => Matches(x, methodSymbol));

        private bool CanPotentiallyRaise(IMethodSymbol methodSymbol) =>
            ReturnsOrAcceptsFormattableType(methodSymbol) ||
            blacklistMethods.Any(x => Matches(x, methodSymbol));

        private static bool ReturnsOrAcceptsFormattableType(IMethodSymbol methodSymbol) =>
            methodSymbol.ReturnType.IsAny(formattableTypes) ||
            methodSymbol.GetParameters().Any(p => p.Type.IsAny(formattableTypes));

        public static bool HasAnyFormatOrCultureParameter(ISymbol method) =>
            method.GetParameters().Any(p => p.Type.IsAny(formatAndCultureType));

        private static bool Matches(MemberDescriptor memberDescriptor, IMethodSymbol methodSymbol) =>
            methodSymbol != null &&
            methodSymbol.ContainingType.Is(memberDescriptor.ContainingType) &&
            methodSymbol.Name == memberDescriptor.Name;
    }
}
