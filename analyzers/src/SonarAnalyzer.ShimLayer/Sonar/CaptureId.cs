/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace StyleCop.Analyzers.Lightup;

public readonly struct CaptureId : IEquatable<CaptureId>
{
    private readonly object instance;   // Underlaying struct holds only internal int Value as the identificator.

    public CaptureId(object instance) =>
        this.instance = instance ?? throw new ArgumentNullException(nameof(instance));

    public override bool Equals(object obj) =>
        obj is CaptureId capture && Equals(capture);

    public bool Equals(CaptureId other) =>
        instance.Equals(other.instance);

    public override int GetHashCode() =>
        instance.GetHashCode();

    public string Serialize() =>
        "#Capture-" + GetHashCode();
}
