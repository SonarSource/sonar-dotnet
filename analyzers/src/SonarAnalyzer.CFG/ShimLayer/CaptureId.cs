/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;

namespace StyleCop.Analyzers.Lightup
{
    public struct CaptureId : IEquatable<CaptureId>
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
    }
}
