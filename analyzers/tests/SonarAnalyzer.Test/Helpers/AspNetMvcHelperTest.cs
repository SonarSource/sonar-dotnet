/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Test.Helpers;

[TestClass]
public class AspNetMvcHelperTest
{
    [DataTestMethod]
    [DataRow("3.0.20105.1")]
    [DataRow(SonarAnalyzer.TestFramework.Common.Constants.NuGetLatestVersion)]
    public void Public_Controller_Methods_Are_EntryPoints(string aspNetMvcVersion)
    {
        const string code = @"
public abstract class Foo : System.Web.Mvc.Controller
{
    public Foo() { }
    public void PublicFoo() { }
    protected void ProtectedFoo() { }
    internal void InternalFoo() { }
    private void PrivateFoo() { }
    public static void StaticFoo() { }
    public virtual void VirtualFoo() { }
    public abstract void AbstractFoo();
    public void InFoo(in string arg) { }
    public void OutFoo(out string arg) { arg = null; }
    public void RefFoo(ref string arg) { }
    public void ReadonlyRefFoo(ref readonly string arg) { }
    public void GenericFoo<T>(T arg) { }
    private class Bar : System.Web.Mvc.Controller
    {
        public void InnerFoo() { }
    }
    [System.Web.Mvc.NonActionAttribute]
    public void PublicNonAction() { }
}";
        var compilation = TestHelper.CompileCS(code, NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion).ToArray()).Model.Compilation;
        var constructorFoo = compilation.GetTypeByMetadataName("Foo").Constructors[0];
        var publicFoo = GetMethodSymbol(compilation, "PublicFoo");
        var protectedFoo = GetMethodSymbol(compilation, "ProtectedFoo");
        var internalFoo = GetMethodSymbol(compilation, "InternalFoo");
        var privateFoo = GetMethodSymbol(compilation, "PrivateFoo");
        var staticFoo = GetMethodSymbol(compilation, "StaticFoo");
        var virtualFoo = GetMethodSymbol(compilation, "VirtualFoo");
        var abstractFoo = GetMethodSymbol(compilation, "AbstractFoo");
        var inFoo = GetMethodSymbol(compilation, "InFoo");
        var outFoo = GetMethodSymbol(compilation, "OutFoo");
        var readonlyRefFoo = GetMethodSymbol(compilation, "ReadonlyRefFoo");
        var refFoo = GetMethodSymbol(compilation, "RefFoo");
        var genericFoo = GetMethodSymbol(compilation, "GenericFoo");
        var innerFoo = GetMethodSymbol(compilation, "InnerFoo");
        var publicNonAction = GetMethodSymbol(compilation, "PublicNonAction");

        AspNetMvcHelper.IsControllerActionMethod(constructorFoo).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(publicFoo).Should().Be(true);
        AspNetMvcHelper.IsControllerActionMethod(protectedFoo).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(internalFoo).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(privateFoo).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(staticFoo).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(inFoo).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(outFoo).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(refFoo).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(readonlyRefFoo).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(staticFoo).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(virtualFoo).Should().Be(true);
        AspNetMvcHelper.IsControllerActionMethod(abstractFoo).Should().Be(true);
        AspNetMvcHelper.IsControllerActionMethod(genericFoo).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(innerFoo).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(publicNonAction).Should().Be(false);
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
}";
        var compilation = TestHelper.CompileCS(code, NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion).ToArray()).Model.Compilation;
        var publicFoo = GetMethodSymbol(compilation, "PublicFoo");
        var publicBar = GetMethodSymbol(compilation, "PublicBar");
        var publicDiz = GetMethodSymbol(compilation, "PublicDiz");
        var publicNonAction = GetMethodSymbol(compilation, "PublicNonAction");

        AspNetMvcHelper.IsControllerActionMethod(publicFoo).Should().Be(true);
        AspNetMvcHelper.IsControllerActionMethod(publicBar).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(publicDiz).Should().Be(false);
        AspNetMvcHelper.IsControllerActionMethod(publicNonAction).Should().Be(false);
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
}";
        var compilation = TestHelper.CompileCS(code, MetadataReferenceFacade.NetStandard.Union(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetMvcVersion)).ToArray())
            .Model
            .Compilation;
        var publicFoo = GetMethodSymbol(compilation, "PublicFoo");
        var publicNonAction = GetMethodSymbol(compilation, "PublicNonAction");

        AspNetMvcHelper.IsControllerActionMethod(publicFoo).Should().Be(true);
        AspNetMvcHelper.IsControllerActionMethod(publicNonAction).Should().Be(false);
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
}";
        var compilation = TestHelper.CompileCS(code, MetadataReferenceFacade.NetStandard.Union(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetMvcVersion)).ToArray())
            .Model
            .Compilation;
        var publicFoo = GetMethodSymbol(compilation, "PublicFoo");

        AspNetMvcHelper.IsControllerActionMethod(publicFoo).Should().Be(false);
    }

    [DataTestMethod]
    [DataRow("2.1.3")]
    [DataRow(Constants.NuGetLatestVersion)]
    public void Constructors_In_Classes_IsControllerMethod_Returns_False(string aspNetMvcVersion)
    {
        const string code = @"
[Microsoft.AspNetCore.Mvc.ControllerAttribute]
public class Foo : Microsoft.AspNetCore.Mvc.ControllerBase
{
        public Foo() { }
}";
        var (tree, semanticModel) = TestHelper.CompileCS(code, MetadataReferenceFacade.NetStandard.Union(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetMvcVersion)).ToArray());
        var methodSymbol = semanticModel.GetDeclaredSymbol(tree.Single<ConstructorDeclarationSyntax>()) as IMethodSymbol;
        methodSymbol.IsControllerActionMethod().Should().Be(false);
    }

    private static IMethodSymbol GetMethodSymbol(Compilation compilation, string name) =>
        (IMethodSymbol)compilation.GetSymbolsWithName(name).Single();
}
