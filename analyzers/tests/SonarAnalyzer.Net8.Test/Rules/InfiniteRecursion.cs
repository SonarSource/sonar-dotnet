extern alias csharp;

using csharp::SonarAnalyzer.Rules.CSharp;
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

namespace SonarAnalyzer.Net8.Test.Rules;

[TestClass]
public class InfiniteRecursionTest
{
    private readonly VerifierBuilder roslynCfg = new VerifierBuilder<InfiniteRecursion>()
        .AddReferences(MetadataReferenceFacade.NetStandard21);

    [TestMethod]
    // see https://github.com/SonarSource/sonar-dotnet/issues/8977
    public void InfiniteRecursion_RoslynCfg_8977() =>
        roslynCfg.AddPaths("InfiniteRecursion_8977.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();
}
