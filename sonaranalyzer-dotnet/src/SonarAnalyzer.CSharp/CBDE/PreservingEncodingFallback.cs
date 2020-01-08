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

using System.Text;

namespace SonarAnalyzer.CBDE
{

    /// <summary>
    /// C# source code can contain any character, but MLIR only handle 8-bits chars. We must therefore encode C# names
    /// so that two different strings in C# always result in two different strings in the generated code (by default, all
    /// unknown characters would be translated to the same one)
    /// The used encoding scheme assumes that the initial string does not contain '.'
    /// </summary>
    internal class PreservingEncodingFallback : EncoderFallback
    {
        public override int MaxCharCount => 4;

        public override EncoderFallbackBuffer CreateFallbackBuffer()
        {
            return new PreservingEncodingBuffer();
        }
    }
}
