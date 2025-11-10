/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Core.Rules;

public sealed record MethodDeclarationInfoComparer : IEqualityComparer<MethodDeclarationInfo>
{
    public bool Equals(MethodDeclarationInfo first, MethodDeclarationInfo second) =>
        (first is null && second is null)
        || (first is not null
            && second is not null
            && first.TypeName == second.TypeName
            && first.MethodName == second.MethodName);

    public int GetHashCode(MethodDeclarationInfo info) =>
        HashCode.Combine(info.TypeName, info.MethodName);
}
