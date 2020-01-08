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

 extern alias csharp;

using System;
using System.Text;
using csharp::SonarAnalyzer.CBDE;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.CBDE
{
    [TestClass]
    public class PreservingEncodingTest
    {
        private readonly Encoding encoder = System.Text.Encoding.GetEncoding("ASCII", new PreservingEncodingFallback(), DecoderFallback.ExceptionFallback);

        public string Encode(string name)
        {
            Byte[] encodedBytes = encoder.GetBytes(name);
            return encoder.GetString(encodedBytes);
        }

        public void checkEncoding(string source, string encoded)
        {
            Assert.AreEqual(encoded, Encode(source));
        }

        [TestMethod]
        public void SimpleCharsAreUnchanged()
        {
            checkEncoding("ABCDEF abcdef", "ABCDEF abcdef");
        }

        [TestMethod]
        public void AccentedCharacterAreChanged()
        {
            checkEncoding("àÅéÈïİøÒùÛçµ", ".E0.C5.E9.C8.EF.130.F8.D2.F9.DB.E7.B5");
        }

        [TestMethod]
        public void CharactersWithLongEncodingAreChanged()
        {
            checkEncoding("𤭢𐐷", ".D852DF62.D801DC37");
        }

        [TestMethod]
        public void StringWithPointsAreNoLongerInjective()
        {
            checkEncoding(".E0", ".E0"); // If the input string contains a point, we are no longer injective
            checkEncoding("à", ".E0");
        }

    }
}
