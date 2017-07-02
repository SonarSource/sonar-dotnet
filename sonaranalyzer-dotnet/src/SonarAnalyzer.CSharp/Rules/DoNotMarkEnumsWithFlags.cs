/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
    public sealed class DoNotMarkEnumsWithFlags : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4070";
        private const string MessageFormat = "Remove the 'FlagsAttribute' from this enum.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var enumDeclaration = (EnumDeclarationSyntax)c.Node;
                    var enumSymbol = c.SemanticModel.GetDeclaredSymbol(enumDeclaration);

                    if (!enumDeclaration.HasFlagsAttribute(c.SemanticModel) ||
                        enumDeclaration.Identifier.IsMissing ||
                        enumSymbol == null)
                    {
                        return;
                    }

                    var membersWithValues = enumSymbol.GetMembers()
                        .OfType<IFieldSymbol>()
                        .Select(member => new { Member = member, Value = GetEnumValueOrDefault(member) })
                        .OrderByDescending(tuple => tuple.Value)
                        .ToList();

                    var allValues = membersWithValues.Select(x => x.Value)
                        .OfType<ulong>()
                        .ToList();

                    var invalidMembers = membersWithValues.Where(tuple => IsInvalidFlagValue(tuple.Value, allValues))
                        .Select(tuple => tuple.Member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation())
                        .WhereNotNull()
                        .ToList();

                    if (invalidMembers.Count > 0)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, enumDeclaration.Identifier.GetLocation(),
                            additionalLocations: invalidMembers));
                    }
                }, SyntaxKind.EnumDeclaration);
        }

        private static ulong? GetEnumValueOrDefault(IFieldSymbol enumMember)
        {
            ulong longValue;
            if (!enumMember.HasConstantValue ||
                !ulong.TryParse(enumMember.ConstantValue.ToString(), out longValue))
            {
                return null;
            }

            return longValue;
        }

        private static bool IsInvalidFlagValue(ulong? enumValue, List<ulong> allValues)
        {
            return !enumValue.HasValue ||
                (!IsZeroOrPowerOfTwo(enumValue.Value) && !IsCombinationOfOtherValues(enumValue.Value, allValues));
        }

        private static bool IsZeroOrPowerOfTwo(ulong value)
        {
            return (value & (value - 1)) == 0;
        }

        private static bool IsCombinationOfOtherValues(ulong value, List<ulong> otherValues)
        {
            // Assume otherValues is not empty and sorted Z -> A
            if (value > otherValues[0])
            {
                return false;
            }

            var newValue = value;
            int currentIndex = 0;
            while (newValue > 0 && currentIndex < otherValues.Count)
            {
                if (otherValues[currentIndex] >= value)
                {
                    currentIndex++;
                    continue;
                }

                newValue -= otherValues[currentIndex];
                currentIndex++;
            }

            return newValue == 0;
        }
    }
}
