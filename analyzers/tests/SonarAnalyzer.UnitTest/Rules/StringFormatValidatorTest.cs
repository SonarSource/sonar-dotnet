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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class StringFormatValidatorTest
    {
        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        [TestCategory("Rule")]
        public void StringFormatRuntimeExceptionFreeValidator(ProjectType projectType) =>
            Verifier.VerifyAnalyzer(@"TestCases\StringFormatRuntimeExceptionFreeValidator.cs", new CS.StringFormatValidator(), TestHelper.ProjectTypeReference(projectType));

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        [TestCategory("Rule")]
        public void StringFormatTypoFreeValidator(ProjectType projectType) =>
            Verifier.VerifyAnalyzer(@"TestCases\StringFormatTypoFreeValidator.cs", new CS.StringFormatValidator(), TestHelper.ProjectTypeReference(projectType));

        [TestMethod]
        [TestCategory("Rule")]
        public void StringFormatEdgeCasesValidator() =>
            Verifier.VerifyAnalyzer(@"TestCases\StringFormatEdgeCasesValidator.cs", new CS.StringFormatValidator());
    }
}
