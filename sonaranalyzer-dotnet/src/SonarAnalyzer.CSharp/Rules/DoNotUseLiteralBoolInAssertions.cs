/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotUseLiteralBoolInAssertions : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2701";
        private const string MessageFormat = "Remove or correct this assertion.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static Dictionary<KnownType, HashSet<string>> trackedTypeAndMethods =
            new Dictionary<KnownType, HashSet<string>>
            {
                [KnownType.Xunit_Assert] = new HashSet<string>
                {
                    "Equal", "False", "NotEqual", "Same", "StrictEqual", "NotSame"
                },

                [KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert] = new HashSet<string>
                {
                    "AreEqual", "AreNotEqual", "AreSame", "IsFalse", "IsTrue"
                },

                [KnownType.NUnit_Framework_Assert] = new HashSet<string>
                {
                    "AreEqual", "AreNotEqual", "AreNotSame", "AreSame", "False",
                    "IsFalse", "IsTrue", "That", "True"
                },

                [KnownType.System_Diagnostics_Debug] = new HashSet<string>
                {
                    "Assert"
                }
            };

        private static readonly ISet<SyntaxKind> boolLiterals =
            new HashSet<SyntaxKind>
            {
                SyntaxKind.TrueLiteralExpression,
                SyntaxKind.FalseLiteralExpression
            };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;

                    var symbolInfo = c.SemanticModel.GetSymbolInfo(invocation);
                    var methodSymbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

                    if (methodSymbol != null &&
                        invocation.ArgumentList != null &&
                        IsTrackedMethod(methodSymbol) &&
                        IsFirstOrSecondArgumentABoolLiteral(invocation.ArgumentList.Arguments) &&
                        !IsWorkingWithNullableType(methodSymbol, invocation.ArgumentList.Arguments, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, invocation.GetLocation()));
                    }
                },
                SyntaxKind.InvocationExpression);
        }

        private bool IsWorkingWithNullableType(ISymbol symbol, SeparatedSyntaxList<ArgumentSyntax> arguments, SemanticModel semanticModel)
        {
            if (!(symbol is IMethodSymbol methodSymbol))
            {
                return false;
            }

            if (methodSymbol.TypeArguments.Length == 1) // We usually expect all comparison test methods to have one generic argument
            {
                // Since we already know we are comparing with bool, no need to check Nullable<bool>, Nullable<T> is enough
                return methodSymbol.TypeArguments[0].OriginalDefinition.Is(KnownType.System_Nullable_T);
            }
            else if (methodSymbol.TypeArguments.Length == 0) // But they can also work with Object (NUnit...)
            {
                if (arguments.Count != 2)
                {
                    return false;
                }
                var nonBoolLiteral = IsBooleanLiteral(arguments[0]) ? arguments[1] : arguments[0];
                var nonBoolType = semanticModel.GetTypeInfo(nonBoolLiteral.Expression);
                return nonBoolType.Type?.OriginalDefinition.Is(KnownType.System_Nullable_T) ?? false;
            }
            else
            {
                // Other case, not handled
                return false;
            }
        }

        private static bool IsFirstOrSecondArgumentABoolLiteral(
            SeparatedSyntaxList<ArgumentSyntax> arguments)
        {
            switch (arguments.Count)
            {
                case 0:  return false;
                case 1:  return IsBooleanLiteral(arguments[0]);
                default: return IsBooleanLiteral(arguments[0]) || IsBooleanLiteral(arguments[1]);
            }
        }

        private static bool IsBooleanLiteral(ArgumentSyntax argument)
        {
            return argument.Expression.IsAnyKind(boolLiterals);
        }

        private static bool IsTrackedMethod(ISymbol methodSymbol)
        {
            return trackedTypeAndMethods
                .Where(kvp => methodSymbol.ContainingType.Is(kvp.Key))
                .Any(kvp => kvp.Value.Contains(methodSymbol.Name));
        }
    }
}
