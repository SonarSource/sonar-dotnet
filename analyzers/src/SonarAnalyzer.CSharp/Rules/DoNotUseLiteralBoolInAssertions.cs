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

using SonarAnalyzer.CFG.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotUseLiteralBoolInAssertions : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2701";
        private const string MessageFormat = "Remove or correct this assertion.";

        private static readonly Dictionary<KnownType, HashSet<string>> TrackedTypeAndMethods =
            new ()
            {
                [KnownType.Xunit_Assert] = new HashSet<string>
                {
                    // "True" is not here because there was no Assert.Fail in Xunit until 2020 and Assert.True(false) was a way to simulate it.
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

        private static readonly ISet<SyntaxKind> BoolLiterals =
            new HashSet<SyntaxKind>
            {
                SyntaxKind.TrueLiteralExpression,
                SyntaxKind.FalseLiteralExpression
            };

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    if (invocation.ArgumentList != null
                        && IsFirstOrSecondArgumentABoolLiteral(invocation.ArgumentList.Arguments)
                        && c.SemanticModel.GetSymbolOrCandidateSymbol(invocation) is IMethodSymbol methodSymbol
                        && IsTrackedMethod(methodSymbol)
                        && !IsWorkingWithNullableType(methodSymbol, invocation.ArgumentList.Arguments, c.SemanticModel))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, invocation.GetLocation()));
                    }
                },
                SyntaxKind.InvocationExpression);

        private static bool IsWorkingWithNullableType(IMethodSymbol methodSymbol, SeparatedSyntaxList<ArgumentSyntax> arguments, SemanticModel semanticModel)
        {
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

        private static bool IsFirstOrSecondArgumentABoolLiteral(SeparatedSyntaxList<ArgumentSyntax> arguments) =>
            arguments.Count switch
            {
                0 => false,
                1 => IsBooleanLiteral(arguments[0]),
                _ => IsBooleanLiteral(arguments[0]) || IsBooleanLiteral(arguments[1]),
            };

        private static bool IsBooleanLiteral(ArgumentSyntax argument) =>
            argument.Expression.IsAnyKind(BoolLiterals);

        private static bool IsTrackedMethod(ISymbol methodSymbol) =>
            TrackedTypeAndMethods
                .Where(kvp => methodSymbol.ContainingType.Is(kvp.Key))
                .Any(kvp => kvp.Value.Contains(methodSymbol.Name));
    }
}
