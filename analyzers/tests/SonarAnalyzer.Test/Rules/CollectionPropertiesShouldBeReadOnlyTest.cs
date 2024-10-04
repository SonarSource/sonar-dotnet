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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class CollectionPropertiesShouldBeReadOnlyTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<CollectionPropertiesShouldBeReadOnly>().AddReferences(MetadataReferenceFacade.SystemRuntimeSerialization);

    [TestMethod]
    public void CollectionPropertiesShouldBeReadOnly() =>
        builder.AddPaths("CollectionPropertiesShouldBeReadOnly.cs")
            .Verify();

#if NET

    [TestMethod]
    public void CollectionPropertiesShouldBeReadOnly_CS_Latest() =>
        builder.AddPaths("CollectionPropertiesShouldBeReadOnly.Latest.cs")
            .WithTopLevelStatements()
            .WithOptions(ParseOptionsHelper.CSharpLatest)
            .Verify();

    [TestMethod]
    public void CollectionPropertiesShouldBeReadOnly_Razor() =>
        builder.AddPaths("CollectionPropertiesShouldBeReadOnly.razor", "CollectionPropertiesShouldBeReadOnly.razor.cs")
               .Verify();

#endif

}
