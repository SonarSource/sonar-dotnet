/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Linq;
using csharp::SonarAnalyzer.ControlFlowGraph.CSharp;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.ControlFlowGraph
{
    [TestClass]
    public class AspNetMvcHelper_IsControllerMethod
    {
        [DataTestMethod]
        [DataRow("3.0.20105.1")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void Public_Controller_Methods_Are_EntryPoints(string aspNetMvcVersion)
        {
            const string code = @"
public class Foo : System.Web.Mvc.Controller
{
    public void PublicFoo() { }
    protected void ProtectedFoo() { }
    internal void InternalFoo() { }
    private void PrivateFoo() { }
    private class Bar : System.Web.Mvb.Controller
    {
        public void InnerFoo() { }
    }
}
";
            var compilation = TestHelper.Compile(code,
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion).ToArray());

            var publicFoo = compilation.GetMethodSymbol("PublicFoo");
            var protectedFoo = compilation.GetMethodSymbol("ProtectedFoo");
            var internalFoo = compilation.GetMethodSymbol("InternalFoo");
            var privateFoo = compilation.GetMethodSymbol("PrivateFoo");
            var innerFoo = compilation.GetMethodSymbol("InnerFoo");

            AspNetMvcHelper.IsControllerMethod(publicFoo).Should().Be(true);
            AspNetMvcHelper.IsControllerMethod(protectedFoo).Should().Be(false);
            AspNetMvcHelper.IsControllerMethod(internalFoo).Should().Be(false);
            AspNetMvcHelper.IsControllerMethod(privateFoo).Should().Be(false);
            AspNetMvcHelper.IsControllerMethod(innerFoo).Should().Be(false);
        }

        [DataTestMethod]
        [DataRow("3.0.20105.1")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void Controller_Methods_Are_EntryPoints(string aspNetMvcVersion)
        {
            const string code = @"
public class Foo : System.Web.Mvc.Controller
{
    public void PublicFoo() { }
}
public class Controller
{
    public void PublicBar() { }
}
public class MyController : Controller
{
    public void PublicDiz() { }
}
";
            var compilation = TestHelper.Compile(code,
                additionalReferences:  NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion).ToArray());

            var publicFoo = compilation.GetMethodSymbol("PublicFoo");
            var publicBar = compilation.GetMethodSymbol("PublicBar");
            var publicDiz = compilation.GetMethodSymbol("PublicDiz");

            AspNetMvcHelper.IsControllerMethod(publicFoo).Should().Be(true);
            AspNetMvcHelper.IsControllerMethod(publicBar).Should().Be(false);
            AspNetMvcHelper.IsControllerMethod(publicDiz).Should().Be(false);
        }

        [DataTestMethod]
        [DataRow("3.0.20105.1")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void Methods_In_Classes_With_ControllerAttribute_Are_EntryPoints(string aspNetMvcVersion)
        {
            const string code = @"
// The Attribute suffix is required because we don't have a reference
// to ASP.NET Core and we cannot do type checking in the test project.
// We will need to convert this test project to .NET Core to do that.
[Microsoft.AspNetCore.Mvc.ControllerAttribute]
public class Foo
{
    public void PublicFoo() { }
}
";
            var compilation = TestHelper.Compile(code,
                additionalReferences:  NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion).ToArray());

            var publicFoo = compilation.GetMethodSymbol("PublicFoo");

            AspNetMvcHelper.IsControllerMethod(publicFoo).Should().Be(true);
        }

        [DataTestMethod]
        [DataRow("3.0.20105.1")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void Methods_In_Classes_With_NonControllerAttribute_Are_Not_EntryPoints(string aspNetMvcVersion)
        {
            const string code = @"
// The Attribute suffix is required because we don't have a reference
// to ASP.NET Core and we cannot do type checking in the test project.
// We will need to convert this test project to .NET Core to do that.
[Microsoft.AspNetCore.Mvc.NonControllerAttribute]
public class Foo : Microsoft.AspNetCore.Mvc.ControllerBase
{
    public void PublicFoo() { }
}
";
            var compilation = TestHelper.Compile(code,
                additionalReferences:  NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion).ToArray());

            var publicFoo = compilation.GetMethodSymbol("PublicFoo");

            AspNetMvcHelper.IsControllerMethod(publicFoo).Should().Be(false);
        }
    }
}
