﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using System.Numerics;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotMarkEnumsWithFlags : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4070";
        private const string MessageFormat = "Remove the 'FlagsAttribute' from this enum.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var enumDeclaration = (EnumDeclarationSyntax)c.Node;
                    var enumSymbol = c.SemanticModel.GetDeclaredSymbol(enumDeclaration);

                    if (!enumDeclaration.HasFlagsAttribute(c.SemanticModel)
                        || enumDeclaration.Identifier.IsMissing
                        || enumSymbol == null)
                    {
                        return;
                    }

                    var membersWithValues = enumSymbol.GetMembers()
                        .OfType<IFieldSymbol>()
                        .Select(member => new { Member = member, Value = GetEnumValueOrDefault(member) })
                        .OrderByDescending(tuple => tuple.Value)
                        .ToList();

                    var allValues = membersWithValues.Select(x => x.Value)
                        .OfType<BigInteger>()
                        .Distinct()
                        .ToList();

                    var invalidMembers = membersWithValues.Where(tuple => !IsValidFlagValue(tuple.Value, allValues))
                        .Select(tuple => tuple.Member.GetFirstSyntaxRef()?.ToSecondaryLocation())
                        .WhereNotNull()
                        .ToList();

                    if (invalidMembers.Count > 0)
                    {
                        c.ReportIssue(Rule, enumDeclaration.Identifier, invalidMembers);
                    }
                }, SyntaxKind.EnumDeclaration);

        private static BigInteger? GetEnumValueOrDefault(IFieldSymbol enumMember) =>
            enumMember.HasConstantValue
                ? enumMember.ConstantValue switch // BigInteger is used here to support enums with base type of ulong. Direct cast is used to avoid string conversion.
                  {
                      byte x => (BigInteger)x,
                      sbyte x => (BigInteger)x,
                      short x => (BigInteger)x,
                      ushort x => (BigInteger)x,
                      int x => (BigInteger)x,
                      uint x => (BigInteger)x,
                      long x => (BigInteger)x,
                      ulong x => (BigInteger)x,
                      _ => null
                  }
                : null;

        private static bool IsValidFlagValue(BigInteger? enumValue, IEnumerable<BigInteger> allValues) =>
            enumValue.HasValue
            && (IsZeroOrPowerOfTwo(enumValue.Value)
                || IsCombinationOfOtherValues(enumValue.Value, allValues));

        private static bool IsZeroOrPowerOfTwo(BigInteger value) =>
            value.IsZero
            || BigInteger.Abs(value).IsPowerOfTwo;

        private static bool IsCombinationOfOtherValues(BigInteger value, IEnumerable<BigInteger> otherValues)
        {
            var currentValue = value;
            foreach (var otherValue in otherValues.SkipWhile(x => value <= x))
            {
                if (otherValue <= currentValue)
                {
                    currentValue ^= otherValue;
                    if (currentValue == 0)
                    {
                        return true;
                    }
                }
            }

            return currentValue == 0;
        }
    }
}
