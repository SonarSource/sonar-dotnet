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

public enum ConstraintKind
{
    // Well-known kinds with irregular names for simplicity
    False,
    True,
    Null,
    NotNull,
    // Everything else
    CollectionEmpty,
    CollectionNotEmpty,
    CryptographicallyStrong,
    CryptographicallyWeak,
    CryptographicallyPredictable,
    CryptographicallyUnpredictable,
    CryptographicKeyStoredSafe,
    CryptographicKeyStoredUnsafe,
    DisposableDisposed,
    InitializationVectorInitialized,
    InitializationVectorNotInitialized,
    LockHeld,
    LockReleased,
    [Obsolete("Old SE engine only")]
    NullableHasValue,
    [Obsolete("Old SE engine only")]
    NullableNoValue,
    Number,
    ParameterReassigned,
    SaltSizeSafe,
    SaltSizeShort,
    SerializationSafe,
    SerializationUnsafe,
    [Obsolete("Old SE engine only")]
    StringEmpty,
    [Obsolete("Old SE engine only")]
    StringFullNotWhiteSpace,
    [Obsolete("Old SE engine only")]
    StringFullOrNull,
    [Obsolete("Old SE engine only")]
    StringFull,
    [Obsolete("Old SE engine only")]
    StringNotWhiteSpace,
    [Obsolete("Old SE engine only")]
    StringWhiteSpace,
}
