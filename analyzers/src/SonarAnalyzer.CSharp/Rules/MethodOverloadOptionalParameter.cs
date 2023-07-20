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

using System.IO;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MethodOverloadOptionalParameter : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3427";
        private const string MessageFormat =
            "This method signature overlaps the one defined on line {0}{1}, the default parameter value {2}.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private class ParameterHidingMethodInfo
        {
            public IParameterSymbol ParameterToReportOn { get; set; }
            public IMethodSymbol HidingMethod { get; set; }
            public IMethodSymbol HiddenMethod { get; set; }
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(
                c =>
                {
                    if (!(c.Symbol is IMethodSymbol methodSymbol) ||
                        methodSymbol.GetInterfaceMember() != null ||
                        methodSymbol.GetOverriddenMember() != null ||
                        !methodSymbol.Parameters.Any(p => p.IsOptional))
                    {
                        return;
                    }

                    var methods = methodSymbol.ContainingType.GetMembers(methodSymbol.Name)
                        .OfType<IMethodSymbol>()
                        .Where(m => m.TypeParameters.Length == methodSymbol.TypeParameters.Length);

                    var hidingInfos = new List<ParameterHidingMethodInfo>();

                    var matchingNamedMethods = methods
                        .Where(m => m.Parameters.Length < methodSymbol.Parameters.Length)
                        .Where(m => !m.Parameters.Any(p => p.IsParams));

                    foreach (var candidateHidingMethod in matchingNamedMethods
                        .Where(candidateHidingMethod => IsMethodHidingOriginal(candidateHidingMethod, methodSymbol))
                        .Where(candidateHidingMethod => methodSymbol.Parameters[candidateHidingMethod.Parameters.Length].IsOptional))
                    {
                        hidingInfos.Add(
                            new ParameterHidingMethodInfo
                            {
                                ParameterToReportOn = methodSymbol.Parameters[candidateHidingMethod.Parameters.Length],
                                HiddenMethod = methodSymbol,
                                HidingMethod = candidateHidingMethod
                            });
                    }

                    foreach (var hidingInfo in hidingInfos)
                    {
                        var syntax = hidingInfo.ParameterToReportOn.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                        if (syntax == null || hidingInfo.HidingMethod.ImplementationSyntax() is not { } hidingMethodSyntax)
                        {
                            continue;
                        }
                        var defaultCanBeUsed = IsMoreParameterAvailableInConflicting(hidingInfo) || !MethodsUsingSameParameterNames(hidingInfo);
                        var isOtherFile = syntax.SyntaxTree.FilePath != hidingMethodSyntax.SyntaxTree.FilePath;

                        c.ReportIssue(CreateDiagnostic(rule, syntax.GetLocation(),
                            hidingMethodSyntax.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                            isOtherFile
                                ? $" in file '{new FileInfo(hidingMethodSyntax.SyntaxTree.FilePath).Name}'"
                                : string.Empty,
                            defaultCanBeUsed
                                ? "can only be used with named arguments"
                                : "can't be used"));
                    }
                },
                SymbolKind.Method);
        }

        private static bool MethodsUsingSameParameterNames(ParameterHidingMethodInfo hidingInfo)
        {
            for (var i = 0; i < hidingInfo.HidingMethod.Parameters.Length; i++)
            {
                if (hidingInfo.HidingMethod.Parameters[i].Name !=
                    hidingInfo.HiddenMethod.Parameters[i].Name)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsMoreParameterAvailableInConflicting(ParameterHidingMethodInfo hidingInfo)
        {
            return hidingInfo.HiddenMethod.Parameters.IndexOf(hidingInfo.ParameterToReportOn) <
                hidingInfo.HiddenMethod.Parameters.Length - 1;
        }

        private static bool IsMethodHidingOriginal(IMethodSymbol candidateHidingMethod, IMethodSymbol method)
        {
            return candidateHidingMethod.Parameters
                    .Zip(method.Parameters, (param1, param2) => new { param1, param2 })
                    .All(p => AreTypesEqual(p.param1.Type, p.param2.Type) &&
                              p.param1.IsOptional == p.param2.IsOptional);
        }

        private static bool AreTypesEqual(ITypeSymbol t1, ITypeSymbol t2)
        {
            return Equals(t1, t2) ||
                   t1.Is(TypeKind.TypeParameter) && t2.Is(TypeKind.TypeParameter) ||
                   AreGenericInstancesTypesEqual(t1, t2);
        }

        private static bool AreGenericInstancesTypesEqual(ITypeSymbol t1, ITypeSymbol t2)
        {
            if (t1.OriginalDefinition != t2.OriginalDefinition)
            {
                return false;
            }
            if (t1 is INamedTypeSymbol named1 && t2 is INamedTypeSymbol named2)
            {
                return named1.TypeArguments.SequenceEqual(named2.TypeArguments,
                    (arg1, arg2) => AreTypesEqual(arg1, arg2));
            }
            return false;
        }
    }
}
