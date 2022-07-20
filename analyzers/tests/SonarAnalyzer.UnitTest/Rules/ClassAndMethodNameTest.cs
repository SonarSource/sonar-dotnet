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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ClassAndMethodNameTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ClassAndMethodName>();

        [Ignore][TestMethod]
        public void ClassAndMethodName_CS() =>
            builderCS.AddPaths("ClassAndMethodName.cs", "ClassAndMethodName.Partial.cs")
                .AddReferences(MetadataReferenceFacade.NETStandard21)
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

        [Ignore][TestMethod]
        public void ClassAndMethodName_InTestProject_CS() =>
            builderCS.AddPaths("ClassAndMethodName.Tests.cs").AddTestReference().WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET

        [Ignore][TestMethod]
        public void ClassAndMethodName_TopLevelStatement_CS() =>
            builderCS.AddPaths("ClassAndMethodName.TopLevelStatement.cs").WithTopLevelStatements().Verify();

        [Ignore][TestMethod]
        public void ClassAndMethodName_TopLevelStatement_InTestProject_CS() =>
            builderCS.AddPaths("ClassAndMethodName.TopLevelStatement.Test.cs").AddTestReference().WithTopLevelStatements().Verify();

        [Ignore][TestMethod]
        public void ClassAndMethodName_Record_CS() =>
            builderCS.AddPaths("ClassAndMethodName.Record.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [Ignore][TestMethod]
        public void ClassAndMethodName_Record_InTestProject_CS() =>
            builderCS.AddPaths("ClassAndMethodName.Record.cs").AddTestReference().WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [Ignore][TestMethod]
        public void ClassAndMethodName_RecordStruct_CS() =>
            builderCS.AddPaths("ClassAndMethodName.RecordStruct.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [Ignore][TestMethod]
        public void ClassAndMethodName_RecordStruct_InTestProject_CS() =>
            builderCS.AddPaths("ClassAndMethodName.RecordStruct.cs").AddTestReference().WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [Ignore][TestMethod]
        public void ClassAndMethodName_MethodName_CSharp9() =>
            builderCS.AddPaths("ClassAndMethodName.MethodName.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [Ignore][TestMethod]
        public void ClassAndMethodName_MethodName_InTestProject_CSharp9() =>
            builderCS.AddPaths("ClassAndMethodName.MethodName.CSharp9.cs").AddTestReference().WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [Ignore][TestMethod]
        public void ClassAndMethodName_MethodName_CSharpPreview() =>
            builderCS.AddPaths("ClassAndMethodName.MethodName.CSharpPreview.cs").WithOptions(ParseOptionsHelper.CSharpPreview).Verify();

#endif

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void ClassAndMethodName_VB(ProjectType projectType) =>
            new VerifierBuilder<VB.ClassName>().AddPaths("ClassAndMethodName.vb").AddReferences(TestHelper.ProjectTypeReference(projectType)).Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void ClassAndMethodName_MethodName(ProjectType projectType) =>
            builderCS.AddPaths("ClassAndMethodName.MethodName.cs", "ClassAndMethodName.MethodName.Partial.cs")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

        [Ignore][TestMethod]
        public void TestSplitToParts() =>
            new[]
            {
                ("foo", new[] { "foo" }),
                ("Foo", new[] { "Foo" }),
                ("FFF", new[] { "FFF" }),
                ("FfF", new[] { "Ff", "F" }),
                ("Ff9F", new[] { "Ff", "9", "F" }),
                ("??", new[] { "?", "?" }),
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
