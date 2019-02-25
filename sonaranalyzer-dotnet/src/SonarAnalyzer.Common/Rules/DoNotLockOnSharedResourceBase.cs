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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class DoNotLockOnSharedResourceBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S2551";
        protected const string MessageFormat = "Lock on a dedicated object instance instead.";

        private static readonly ImmutableArray<KnownType> _invalidLockTypes =
           ImmutableArray.Create(
               KnownType.System_String,
               KnownType.System_Type
           );

        protected static bool IsLockOnForbiddenKnownType(SyntaxNode expression, SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(expression).Type.IsAny(_invalidLockTypes);
    }
}
