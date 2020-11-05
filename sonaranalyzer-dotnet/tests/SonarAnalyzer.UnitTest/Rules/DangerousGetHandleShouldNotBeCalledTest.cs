﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using csharp = SonarAnalyzer.Rules.CSharp;
using vbnet = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DangerousGetHandleShouldNotBeCalledTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void DangerousGetHandleShouldNotBeCalled_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\DangerousGetHandleShouldNotBeCalled.cs",
                new csharp.DangerousGetHandleShouldNotBeCalled(),
                additionalReferences: MetadataReferenceFacade.GetMicrosoftWin32Registry());

        [TestMethod]
        [TestCategory("Rule")]
        public void DangerousGetHandleShouldNotBeCalled_CS_CSharp9() =>
            Verifier.VerifyAnalyzer(@"TestCases\DangerousGetHandleShouldNotBeCalled.CSharp9.cs",
                new csharp.DangerousGetHandleShouldNotBeCalled(),
                ParseOptionsHelper.FromCSharp9,
                OutputKind.ConsoleApplication,
                additionalReferences: MetadataReferenceFacade.GetMicrosoftWin32Registry());

        [TestMethod]
        [TestCategory("Rule")]
        public void DangerousGetHandleShouldNotBeCalled_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\DangerousGetHandleShouldNotBeCalled.vb",
                new vbnet.DangerousGetHandleShouldNotBeCalled());
    }
}

