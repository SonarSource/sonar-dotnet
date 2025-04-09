/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using System.IO;

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MethodOverloadOptionalParameter : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3427";
        private const string MessageFormat =
            "This method signature overlaps the one defined on line {0}{1}, the default parameter value {2}.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                c =>
                {
                    if (c.Symbol is not IMethodSymbol methodSymbol
                        || ShouldSkip(methodSymbol))
                    {
                        return;
                    }

                    foreach (var info in GetParameterHidingInfo(methodSymbol))
                    {
                        ReportIssue(c, info);
                    }
                },
                SymbolKind.Method);

        private static void ReportIssue(SonarSymbolReportingContext c, ParameterHidingMethodInfo hidingInfo)
        {
            var syntax = hidingInfo.ParameterToReportOn.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            if (syntax == null || hidingInfo.HidingMethod.ImplementationSyntax() is not { } hidingMethodSyntax)
            {
                return;
            }

            var defaultCanBeUsed = IsMoreParameterAvailableInConflicting(hidingInfo) || !MethodsUsingSameParameterNames(hidingInfo);
            var isOtherFile = syntax.SyntaxTree.FilePath != hidingMethodSyntax.SyntaxTree.FilePath;

            c.ReportIssue(
                Rule,
                syntax,
                (hidingMethodSyntax.GetLocation().GetMappedLineSpan().StartLinePosition.Line + 1).ToString(),
                isOtherFile
                    ? $" in file '{new FileInfo(hidingMethodSyntax.SyntaxTree.FilePath).Name}'"
                    : string.Empty,
                defaultCanBeUsed
                    ? "can only be used with named arguments"
                    : "can't be used");
        }

        private static List<ParameterHidingMethodInfo> GetParameterHidingInfo(IMethodSymbol methodSymbol) =>
            methodSymbol.ContainingType
                        .GetMembers(methodSymbol.Name)
                        .OfType<IMethodSymbol>()
                        .Where(m => m.TypeParameters.Length == methodSymbol.TypeParameters.Length)
                        .Where(m => m.Parameters.Length < methodSymbol.Parameters.Length)
                        .Where(m => !m.Parameters.Any(p => p.IsParams))
                        .Where(candidateHidingMethod => IsMethodHidingOriginal(candidateHidingMethod, methodSymbol))
                        .Where(candidateHidingMethod => methodSymbol.Parameters[candidateHidingMethod.Parameters.Length].IsOptional)
                        .Select(candidateHidingMethod => new ParameterHidingMethodInfo
                                                         {
                                                             ParameterToReportOn = methodSymbol.Parameters[candidateHidingMethod.Parameters.Length],
                                                             HiddenMethod = methodSymbol,
                                                             HidingMethod = candidateHidingMethod
                                                         })
                        .ToList();

        private static bool MethodsUsingSameParameterNames(ParameterHidingMethodInfo hidingInfo)
        {
            for (var i = 0; i < hidingInfo.HidingMethod.Parameters.Length; i++)
            {
                if (hidingInfo.HidingMethod.Parameters[i].Name != hidingInfo.HiddenMethod.Parameters[i].Name)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsMoreParameterAvailableInConflicting(ParameterHidingMethodInfo hidingInfo) =>
            hidingInfo.HiddenMethod.Parameters.IndexOf(hidingInfo.ParameterToReportOn) < hidingInfo.HiddenMethod.Parameters.Length - 1;

        private static bool IsMethodHidingOriginal(IMethodSymbol candidateHidingMethod, IMethodSymbol method) =>
            candidateHidingMethod.Parameters
                                 .Zip(method.Parameters, (param1, param2) => new { param1, param2 })
                                 .All(p => AreTypesEqual(p.param1.Type, p.param2.Type)
                                           && p.param1.IsOptional == p.param2.IsOptional);

        private static bool AreTypesEqual(ITypeSymbol t1, ITypeSymbol t2) =>
            Equals(t1, t2)
            || (t1.Is(TypeKind.TypeParameter) && t2.Is(TypeKind.TypeParameter))
            || AreGenericInstancesTypesEqual(t1, t2);

        private static bool AreGenericInstancesTypesEqual(ITypeSymbol t1, ITypeSymbol t2)
        {
            if (!t1.OriginalDefinition.Equals(t2.OriginalDefinition))
            {
                return false;
            }
            if (t1 is INamedTypeSymbol named1 && t2 is INamedTypeSymbol named2)
            {
                return named1.TypeArguments.SequenceEqual(named2.TypeArguments, AreTypesEqual);
            }
            return false;
        }

        private static bool ShouldSkip(IMethodSymbol methodSymbol) =>
            methodSymbol.InterfaceMembers().Any()
            || methodSymbol.GetOverriddenMember() is not null
            || !methodSymbol.Parameters.Any(p => p.IsOptional);

        private sealed class ParameterHidingMethodInfo
        {
            public IParameterSymbol ParameterToReportOn { get; init; }
            public IMethodSymbol HidingMethod { get; init; }
            public IMethodSymbol HiddenMethod { get; init; }
        }
    }
}
