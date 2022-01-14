﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class StaticFieldInitializerOrderTest
    {
        [TestMethod]
        public void StaticFieldInitializerOrder() =>
            OldVerifier.VerifyAnalyzer(
                new[]
                {
                    @"TestCases\StaticFieldInitializerOrder.cs",
                    @"TestCases\StaticFieldInitializerOrder_PartialClass.cs"
                },
                new StaticFieldInitializerOrder(),
                ParseOptionsHelper.FromCSharp8,
                MetadataReferenceFacade.NETStandard21);

#if NET
        [TestMethod]
        public void StaticFieldInitializerOrder_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\StaticFieldInitializerOrder.CSharp9.cs", new StaticFieldInitializerOrder());

        [TestMethod]
        public void StaticFieldInitializerOrder_CSharp10() =>
            OldVerifier.VerifyAnalyzerFromCSharp10Library(@"TestCases\StaticFieldInitializerOrder.CSharp10.cs", new StaticFieldInitializerOrder());
#endif
    }
}
