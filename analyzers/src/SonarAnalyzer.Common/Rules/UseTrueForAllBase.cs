﻿/*
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

namespace SonarAnalyzer.Rules;

public abstract class UseTrueForAllBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6603";

    protected override string MessageFormat => "The collection-specific \"TrueForAll\" method should be used instead of the \"All\" extension";

    protected UseTrueForAllBase() : base(DiagnosticId) { }

    private static readonly ImmutableArray<KnownType> TargetTypes = ImmutableArray.Create(
        KnownType.System_Array,
        KnownType.System_Collections_Generic_List_T,
        KnownType.System_Collections_Immutable_ImmutableList);

    protected static bool IsCorrectType(SyntaxNode left, SemanticModel model) =>
        model.GetTypeInfo(left).Type is { } type && type.DerivesFromAny(TargetTypes);

    protected static bool IsCorrectCall(SyntaxNode right, SemanticModel model) =>
        model.GetSymbolInfo(right).Symbol is IMethodSymbol method
        && method.IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T);
}
