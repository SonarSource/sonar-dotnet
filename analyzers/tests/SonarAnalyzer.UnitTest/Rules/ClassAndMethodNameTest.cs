/*
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

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ClassAndMethodNameTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ClassAndMethodName>();

        [TestMethod]
        public void ClassName_CS() =>
            builderCS.AddPaths("ClassName.cs", "ClassName.Partial.cs")
                .AddReferences(MetadataReferenceFacade.NETStandard21)
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

        [TestMethod]
        public void ClassName_InTestProject_CS() =>
            builderCS.AddPaths("ClassName.Tests.cs").AddTestReference().WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void ClassName_TopLevelStatement_CS() =>
            builderCS.AddPaths("ClassName.TopLevelStatement.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void ClassName_TopLevelStatement_InTestProject_CS() =>
            builderCS.AddPaths("ClassName.TopLevelStatement.Test.cs").AddTestReference().WithTopLevelStatements().Verify();

        [TestMethod]
        public void RecordName_CS() =>
            builderCS.AddPaths("RecordName.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void RecordName_InTestProject_CS() =>
            builderCS.AddPaths("RecordName.cs").AddTestReference().WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void RecordStructName_CS() =>
            builderCS.AddPaths("RecordStructName.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void RecordStructName_InTestProject_CS() =>
            builderCS.AddPaths("RecordStructName.cs").AddTestReference().WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void MethodName_CSharp9() =>
            builderCS.AddPaths("MethodName.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void MethodName_InTestProject_CSharp9() =>
            builderCS.AddPaths("MethodName.CSharp9.cs").AddTestReference().WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void MethodName_CSharpPreview() =>
            builderCS.AddPaths("MethodName.CSharpPreview.cs").WithOptions(ParseOptionsHelper.CSharpPreview).Verify();

#endif

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void ClassName_VB(ProjectType projectType) =>
            new VerifierBuilder<VB.ClassName>().AddPaths("ClassName.vb").AddReferences(TestHelper.ProjectTypeReference(projectType)).Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void MethodName(ProjectType projectType) =>
            builderCS.AddPaths("MethodName.cs", "MethodName.Partial.cs").AddReferences(TestHelper.ProjectTypeReference(projectType)).WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

        [TestMethod]
        public void TestSplitToParts() =>
            new[]
            {
                ("foo", new[] { "foo" }),
                ("Foo", new[] { "Foo" }),
                ("FFF", new[] { "FFF" }),
                ("FfF", new[] { "Ff", "F" }),
                ("Ff9F", new[] { "Ff", "9", "F" }),
                ("你好", new[] { "你", "好" }),
                ("FFf", new[] { "F", "Ff" }),
                (string.Empty,  Array.Empty<string>()),
                ("FF9d", new[] { "FF", "9", "d" }),
                ("y2x5__w7", new[] { "y", "2", "x", "5", "_", "_", "w", "7" }),
                ("3%c#account", new[] { "3", "%", "c", "#", "account" }),
            }
            .Select(x =>
            (
                actual: CS.ClassAndMethodName.SplitToParts(x.Item1).ToArray(),
                expected: x.Item2))
            .ToList()
            .ForEach(x => x.actual.Should().Equal(x.expected));
    }
}
