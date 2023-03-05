/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.Text.RegularExpressions;
using SonarAnalyzer.RegularExpressions;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules;

[TestClass]
public class RegexMustHaveValidSyntaxTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.RegexMustHaveValidSyntax>()
        .WithBasePath("RegularExpressions")
        .AddReferences(MetadataReferenceFacade.RegularExpressions)
        .AddReferences(NuGetMetadataReference.SystemComponentModelAnnotations());

    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.RegexMustHaveValidSyntax>()
        .WithBasePath("RegularExpressions")
        .AddReferences(MetadataReferenceFacade.RegularExpressions)
        .AddReferences(NuGetMetadataReference.SystemComponentModelAnnotations());

    [TestMethod]
    public void RegexMustHaveValidSyntax_CS() =>
        builderCS.AddPaths("RegexMustHaveValidSyntax.cs").Verify();

#if NET

    [TestMethod]
    public void RegexMustHaveValidSyntax_CSharp9() =>
        builderCS.AddPaths("RegexMustHaveValidSyntax.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

#endif

    [TestMethod]
    public void RegexMustHaveValidSyntax_VB() =>
        builderVB.AddPaths("RegexMustHaveValidSyntax.vb").Verify();

    [DataTestMethod]
    [DataRow("[A", RegexOptions.None)]
#if NET
    [DataRow(@"^([0-9]{2})(?<!00)$", RegexOptions.NonBacktracking)]
#endif
    public void Invalid_input_is_detected(string pattern, RegexOptions options) =>
        new RegexContext(null, pattern, null, options).ParseError.Should().NotBeNull();
}
