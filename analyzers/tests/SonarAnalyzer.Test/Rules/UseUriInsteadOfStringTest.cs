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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class UseUriInsteadOfStringTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<UseUriInsteadOfString>().AddReferences(MetadataReferenceFacade.SystemDrawing);

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void UseUriInsteadOfString(ProjectType projectType) =>
            builder.AddPaths("UseUriInsteadOfString.cs").AddReferences(TestHelper.ProjectTypeReference(projectType)).Verify();

#if NET

        [TestMethod]
        public void UseUriInsteadOfString_TopLevelStatements() =>
            builder.AddPaths("UseUriInsteadOfString.TopLevelStatements.cs")
                .WithTopLevelStatements()
                .Verify();

        [TestMethod]
        public void UseUriInsteadOfString_Latest() =>
            builder.AddPaths("UseUriInsteadOfString.Latest.cs", "UseUriInsteadOfString.Latest.Partial.cs")
                .WithOptions(ParseOptionsHelper.CSharpLatest)
                .Verify();

#endif

        [TestMethod]
        public void UseUriInsteadOfString_InvalidCode() =>
            builder.AddSnippet(@"
public class NoMembers
{
}

public class InvalidCode : NoMembers
{
    public override string UriProperty { get; set; }    // Error [CS0115] 'Bar.UriProperty': no suitable method found to override
    public override string UriMethod() => """";         // Error [CS0115] 'Bar.UriMethod()': no suitable method found to override

    public void Main()
    {
        Uri.TryCreate(new object(), UriKind.Absolute, out result); // Compliant - invalid code
        // Error@-1 [CS0103 ]The name 'UriKind' does not exist in the current context
        // Error@-2 [CS0103] The name 'Uri' does not exist in the current context
        // Error@-3 [CS0103] The name 'result' does not exist in the current context
    }
}").Verify();
    }
}
