/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Semantics;

[TestClass]
public class KnownMethodsTest
{
    [TestMethod]
    public  void IsMainMethod_Null_ShouldBeFalse() =>
        KnownMethods.IsMainMethod(null).Should().BeFalse();

    [TestMethod]
    public void IsObjectEquals_Null_ShouldBeFalse() =>
        KnownMethods.IsObjectEquals(null).Should().BeFalse();

    [TestMethod]
    public void IsStaticObjectEquals_Null_ShouldBeFalse() =>
        KnownMethods.IsStaticObjectEquals(null).Should().BeFalse();

    [TestMethod]
    public void IsObjectGetHashCode_Null_ShouldBeFalse() =>
        KnownMethods.IsObjectGetHashCode(null).Should().BeFalse();

    [TestMethod]
    public void IsObjectToString_Null_ShouldBeFalse() =>
        KnownMethods.IsObjectToString(null).Should().BeFalse();

    [TestMethod]
    public void IsIAsyncDisposableDisposeAsync_Null_ShouldBeFalse() =>
        KnownMethods.IsIAsyncDisposableDisposeAsync(null).Should().BeFalse();

    [TestMethod]
    public void IsGetObjectData_Null_ShouldBeFalse() =>
        KnownMethods.IsGetObjectData(null).Should().BeFalse();

    [TestMethod]
    public void IsSerializationConstructor_Null_ShouldBeFalse() =>
        KnownMethods.IsSerializationConstructor(null).Should().BeFalse();

    [TestMethod]
    public void IsArrayClone_Null_ShouldBeFalse() =>
        KnownMethods.IsArrayClone(null).Should().BeFalse();

    [TestMethod]
    public void IsRecordPrintMembers_Null_ShouldBeFalse() =>
        KnownMethods.IsRecordPrintMembers(null).Should().BeFalse();

    [TestMethod]
    public void IsGcSuppressFinalize_Null_ShouldBeFalse() =>
        KnownMethods.IsGcSuppressFinalize(null).Should().BeFalse();

    [TestMethod]
    public void IsDebugAssert_Null_ShouldBeFalse() =>
        KnownMethods.IsDebugAssert(null).Should().BeFalse();

    [TestMethod]
    public void IsDiagnosticDebugMethod_Null_ShouldBeFalse() =>
        KnownMethods.IsDiagnosticDebugMethod(null).Should().BeFalse();

    [TestMethod]
    public void IsOperatorBinaryPlus_Null_ShouldBeFalse() =>
        KnownMethods.IsOperatorBinaryPlus(null).Should().BeFalse();

    [TestMethod]
    public void IsOperatorBinaryMinus_Null_ShouldBeFalse() =>
        KnownMethods.IsOperatorBinaryMinus(null).Should().BeFalse();

    [TestMethod]
    public void IsOperatorBinaryMultiply_Null_ShouldBeFalse() =>
        KnownMethods.IsOperatorBinaryMultiply(null).Should().BeFalse();

    [TestMethod]
    public void IsOperatorBinaryDivide_Null_ShouldBeFalse() =>
        KnownMethods.IsOperatorBinaryDivide(null).Should().BeFalse();

    [TestMethod]
    public void IsOperatorBinaryModulus_Null_ShouldBeFalse() =>
        KnownMethods.IsOperatorBinaryModulus(null).Should().BeFalse();

    [TestMethod]
    public void IsOperatorEquals_Null_ShouldBeFalse() =>
        KnownMethods.IsOperatorEquals(null).Should().BeFalse();

    [TestMethod]
    public void IsOperatorNotEquals_Null_ShouldBeFalse() =>
        KnownMethods.IsOperatorNotEquals(null).Should().BeFalse();

    [TestMethod]
    public void IsConsoleWriteLine_Null_ShouldBeFalse() =>
        KnownMethods.IsConsoleWriteLine(null).Should().BeFalse();

    [TestMethod]
    public void IsConsoleWrite_Null_ShouldBeFalse() =>
        KnownMethods.IsConsoleWrite(null).Should().BeFalse();

    [TestMethod]
    public void IsListAddRange_Null_ShouldBeFalse() =>
        KnownMethods.IsListAddRange(null).Should().BeFalse();

    [TestMethod]
    public void IsEventHandler_Null_ShouldBeFalse() =>
        KnownMethods.IsEventHandler(null).Should().BeFalse();

    [TestMethod]
    public void IsEnumerableConcat_Null_ShouldBeFalse() =>
        KnownMethods.IsEnumerableConcat(null).Should().BeFalse();

    [TestMethod]
    public void Symbol_IsProbablyEventHandler()
    {
        var snippet = new SnippetCompiler("""
            public class Sample
            {
                public void Method() { }
                public void EventHandler(object o, System.EventArgs args){}
            }
            """);
        snippet.MethodSymbol("Sample.Method").IsEventHandler().Should().BeFalse();
        snippet.MethodSymbol("Sample.EventHandler").IsEventHandler().Should().BeTrue();
    }

    [TestMethod]
    public void Symbol_IsProbablyEventHandler_ResolveEventHandler()
    {
        var snippet = new SnippetCompiler("""
            using System;
            using System.Reflection;
            public class AssemblyLoad
            {
                public AssemblyLoad()
                {
                    AppDomain.CurrentDomain.AssemblyResolve += LoadAnyVersion;
                }
                Assembly LoadAnyVersion(object sender, ResolveEventArgs args) => null;
            }
            """);
        snippet.MethodSymbol("AssemblyLoad.LoadAnyVersion").IsEventHandler().Should().BeTrue();
    }
}
