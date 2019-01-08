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

using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
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
    private class Bar : System.Web.Mvc.Controller
    {
        public void InnerFoo() { }
    }
    [System.Web.Mvc.NonActionAttribute]
    public void PublicNonAction() { }
}
";
            var compilation = TestHelper.Compile(code,
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion).ToArray());

            var publicFoo = compilation.GetMethodSymbol("PublicFoo");
            var protectedFoo = compilation.GetMethodSymbol("ProtectedFoo");
            var internalFoo = compilation.GetMethodSymbol("InternalFoo");
            var privateFoo = compilation.GetMethodSymbol("PrivateFoo");
            var innerFoo = compilation.GetMethodSymbol("InnerFoo");
            var publicNonAction = compilation.GetMethodSymbol("PublicNonAction");

            AspNetMvcHelper.IsControllerMethod(publicFoo).Should().Be(true);
            AspNetMvcHelper.IsControllerMethod(protectedFoo).Should().Be(false);
            AspNetMvcHelper.IsControllerMethod(internalFoo).Should().Be(false);
            AspNetMvcHelper.IsControllerMethod(privateFoo).Should().Be(false);
            AspNetMvcHelper.IsControllerMethod(innerFoo).Should().Be(false);
            AspNetMvcHelper.IsControllerMethod(publicNonAction).Should().Be(false);
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
    [System.Web.Mvc.NonActionAttribute]
    public void PublicNonAction() { }
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
            var publicNonAction = compilation.GetMethodSymbol("PublicNonAction");

            AspNetMvcHelper.IsControllerMethod(publicFoo).Should().Be(true);
            AspNetMvcHelper.IsControllerMethod(publicBar).Should().Be(false);
            AspNetMvcHelper.IsControllerMethod(publicDiz).Should().Be(false);
            AspNetMvcHelper.IsControllerMethod(publicNonAction).Should().Be(false);
        }

        [DataTestMethod]
        [DataRow("2.1.3")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void Methods_In_Classes_With_ControllerAttribute_Are_EntryPoints(string aspNetMvcVersion)
        {
            const string code = @"
[Microsoft.AspNetCore.Mvc.ControllerAttribute]
public class Foo
{
    public void PublicFoo() { }
    [Microsoft.AspNetCore.Mvc.NonActionAttribute]
    public void PublicNonAction() { }
}
";
            var compilation = TestHelper.Compile(code,
                additionalReferences:
                    FrameworkMetadataReference.Netstandard
                        .Union(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetMvcVersion))
                        .ToArray());

            var publicFoo = compilation.GetMethodSymbol("PublicFoo");
            var publicNonAction = compilation.GetMethodSymbol("PublicNonAction");

            AspNetMvcHelper.IsControllerMethod(publicFoo).Should().Be(true);
            AspNetMvcHelper.IsControllerMethod(publicNonAction).Should().Be(false);
        }

        [DataTestMethod]
        [DataRow("2.1.3")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void Methods_In_Classes_With_NonControllerAttribute_Are_Not_EntryPoints(string aspNetMvcVersion)
        {
            const string code = @"
[Microsoft.AspNetCore.Mvc.NonControllerAttribute]
public class Foo : Microsoft.AspNetCore.Mvc.ControllerBase
{
    public void PublicFoo() { }
}
";
            var compilation = TestHelper.Compile(code,
                additionalReferences:
                    FrameworkMetadataReference.Netstandard
                        .Union(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetMvcVersion))
                        .ToArray());

            var publicFoo = compilation.GetMethodSymbol("PublicFoo");

            AspNetMvcHelper.IsControllerMethod(publicFoo).Should().Be(false);
        }
    }
}
