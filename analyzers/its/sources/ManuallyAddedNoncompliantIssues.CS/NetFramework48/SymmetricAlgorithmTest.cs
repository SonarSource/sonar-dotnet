/*
 * SonarQube, open source software quality management tool.
 * Copyright (C) 2008-2020 SonarSource
 * mailto:contact AT sonarsource DOT com
 *
 * SonarQube is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * SonarQube is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Security.Cryptography;

namespace NetFramework48
{
    public class SymmetricAlgorithmTest
    {
        public static void TestCases()
        {
            using (var sa = SymmetricAlgorithm.Create("Rijndael"))
            {
                sa.CreateEncryptor();
                sa.CreateEncryptor(sa.Key, new byte[16]); // Noncompliant (S3329) {{Use a dynamically-generated, random IV.}}
            }
        }
    }
}
