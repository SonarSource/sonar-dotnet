/*
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

namespace SonarAnalyzer.SymbolicExecution.Constraints;

public sealed class CollectionConstraint : SymbolicConstraint
{
    public static readonly CollectionConstraint Empty = new(ConstraintKind.CollectionEmpty);
    public static readonly CollectionConstraint NotEmpty = new(ConstraintKind.CollectionNotEmpty);

    public override SymbolicConstraint Opposite =>
        // x.Count == 0 ? Empty : NotEmpty
        // x.Count == 5 ? NotEmpty : <unknown, could be Empty or NotEmpty here>
        this == Empty ? NotEmpty : null;

    private CollectionConstraint(ConstraintKind kind) : base(kind) { }
}
