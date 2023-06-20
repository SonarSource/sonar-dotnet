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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules;

[TestClass]
public class TimestampsShouldNotBeUsedAsPrimaryKeysTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<TimestampsShouldNotBeUsedAsPrimaryKeys>();

    [TestMethod]
    public void TimestampsShouldNotBeUsedAsPrimaryKeys_CSharp_Core() =>
        builder
            .AddPaths("TimestampsShouldNotBeUsedAsPrimaryKeys.cs")
            .AddEntityFrameworkReference()
            .Verify();

    [TestMethod]
    public void TimestampsShouldNotBeUsedAsPrimaryKeys_CSharp9_Core() =>
        builder
            .AddPaths("TimestampsShouldNotBeUsedAsPrimaryKeys.CSharp9.cs")
            .AddEntityFrameworkReference()
            .WithOptions(ParseOptionsHelper.FromCSharp9)
            .Verify();
}

internal static class BuilderExtensions
{
    public static VerifierBuilder AddEntityFrameworkReference(this VerifierBuilder builder) =>
        builder
            .AddReferences(NuGetMetadataReference.SystemComponentModelAnnotations())

#if NET
            .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCore("7.0.0"));
#else
            .AddReferences(NuGetMetadataReference.MicrosoftEntityFramework("6.0.0"));
#endif

}
