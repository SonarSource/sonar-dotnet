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
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class CommentedOutCodeTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void CommentedOutCode()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CommentedOutCode.cs", new CommentedOutCode());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CommentedOutCode_NoDocumentation()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CommentedOutCode.cs", new CommentedOutCode(),
                new[] { new CSharpParseOptions(documentationMode: Microsoft.CodeAnalysis.DocumentationMode.None) });
        }
    }
}
