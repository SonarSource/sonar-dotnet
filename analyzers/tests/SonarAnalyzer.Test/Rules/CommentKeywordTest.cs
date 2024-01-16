/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class CommentKeywordTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.CommentKeyword>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.CommentKeyword>();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void CommentTodo_CS(ProjectType projectType) =>
            builderCS.AddPaths("CommentTodo.cs")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void CommentFixme_CS(ProjectType projectType) =>
            builderCS.AddPaths("CommentFixme.cs")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void CommentTodo_VB(ProjectType projectType) =>
            builderVB.AddPaths("CommentTodo.vb")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void CommentFixme_VB(ProjectType projectType) =>
            builderVB.AddPaths("CommentFixme.vb")
                .AddReferences(TestHelper.ProjectTypeReference(projectType))
                .Verify();
    }
}
