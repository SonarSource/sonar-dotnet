/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.TestFramework.Extensions;

namespace SonarAnalyzer.ShimLayer.Generator.Snippets.Test;

[TestClass]
public class MethodWrapSnippetTest
{
    [TestMethod]
    public void GenericMethod_NormalReturnType() =>
        AssertMemberDeclaration(nameof(Sample.NormalReturnType), "public static String NormalReturnType<T>() => NormalReturnTypeAccessor_GenericStore<T>.NormalReturnTypeAccessor();");

    [TestMethod]
    public void GenericMethod_GenericReturnType() =>
        AssertMemberDeclaration(nameof(Sample.GenericReturnType), "public static T GenericReturnType<T>() => GenericReturnTypeAccessor_GenericStore<T>.GenericReturnTypeAccessor();");

    [TestMethod]
    public void GenericMethod_GenericParameter() =>
        AssertMemberDeclaration(
            nameof(Sample.GenericParameter),
            "public static Void GenericParameter<T>(T parameter) => GenericParameterAccessor_GenericStore<T>.GenericParameterAccessor(parameter);");

    [TestMethod]
    public void GenericMethod_MultipleTypeArguments() =>
        AssertMemberDeclaration(
            nameof(Sample.MultipleTypeArguments),
            "public static Void MultipleTypeArguments<TFirst, TSecond, TThird>(TFirst first, TSecond second, TThird third) => MultipleTypeArgumentsAccessor_GenericStore<TFirst, TSecond, TThird>.MultipleTypeArgumentsAccessor(first, second, third);");

    [TestMethod]
    public void GenericConstraints_Class() =>
        AssertMemberDeclaration(nameof(Sample.ConstraintsClass), "public static Void ConstraintsClass<T>() where T : class => ConstraintsClassAccessor_GenericStore<T>.ConstraintsClassAccessor();");

    [TestMethod]
    public void GenericConstraints_Struct() =>
        AssertMemberDeclaration(
            nameof(Sample.ConstraintsStruct),
            "public static Void ConstraintsStruct<T>() where T : struct => ConstraintsStructAccessor_GenericStore<T>.ConstraintsStructAccessor();");

    [TestMethod]
    public void GenericConstraints_New() =>
        AssertMemberDeclaration(nameof(Sample.ConstraintsNew), "public static Void ConstraintsNew<T>() where T : new() => ConstraintsNewAccessor_GenericStore<T>.ConstraintsNewAccessor();");

    [TestMethod]
    public void GenericConstraints_Types() =>
        AssertMemberDeclaration(
            nameof(Sample.ConstraintsTypes),
            "public static Void ConstraintsTypes<T>() where T : Base, IFaceFirst, IFaceSecond => ConstraintsTypesAccessor_GenericStore<T>.ConstraintsTypesAccessor();");

    [TestMethod]
    public void GenericConstraints_CombinationClass() =>
        AssertMemberDeclaration(
            nameof(Sample.ConstraintsCombinationClass),
            "public static Void ConstraintsCombinationClass<T>() where T : class, IFaceFirst, new() => ConstraintsCombinationClassAccessor_GenericStore<T>.ConstraintsCombinationClassAccessor();");

    [TestMethod]
    public void GenericConstraints_CombinationStruct() =>
        AssertMemberDeclaration(
            nameof(Sample.ConstraintsCombinationStruct),
            "public static Void ConstraintsCombinationStruct<T>() where T : struct, IFaceFirst => ConstraintsCombinationStructAccessor_GenericStore<T>.ConstraintsCombinationStructAccessor();");

    [TestMethod]
    public void GenericConstraints_Enum() =>
        AssertMemberDeclaration(nameof(Sample.ConstraintsEnum), "public static Void ConstraintsEnum<T>() where T : Enum => ConstraintsEnumAccessor_GenericStore<T>.ConstraintsEnumAccessor();");

    [TestMethod]
    public void Parameter_Out() =>
        AssertMemberDeclaration(nameof(Sample.Out), "public static Void Out(out int first, int second, out int third) => OutAccessor(out first, second, out third);");

    [TestMethod]
    public void Parameter_Params() =>
        AssertMemberDeclaration(nameof(Sample.Params), "public static Void Params(int first, params int[] others) => ParamsAccessor(first, others);");

    private static void AssertMemberDeclaration(string methodName, string expected) =>
        CreateSut(methodName).MemberDeclaration(0).Should().BeIgnoringLineEndings(expected);

    private static MethodWrapSnippet CreateSut(string methodName)
    {
        var method = typeof(Sample).GetMethod(methodName);
        return new(
            new NoChangeStrategy(typeof(Sample)),
            new(method, false, methodName + "Accessor"),
            new NoChangeStrategy(method.ReturnType),
            []);
    }

#pragma warning disable S1309   // Do not suppress issues (inception)
#pragma warning disable S1186   // Add comment why method is empty
#pragma warning disable S2094   // Empty class
#pragma warning disable S2326   // Unused T
#pragma warning disable S1172   // Unused parameter Sonar
#pragma warning disable IDE0060 // Unused parameter IDE
    private static class Sample
    {
        public static string NormalReturnType<T>() => null;

        public static T GenericReturnType<T>() => default;

        public static void GenericParameter<T>(T parameter) { }

        public static void MultipleTypeArguments<TFirst, TSecond, TThird>(TFirst first, TSecond second, TThird third) { }

        public static void ConstraintsClass<T>() where T : class { }

        public static void ConstraintsStruct<T>() where T : struct { }

        public static void ConstraintsNew<T>() where T : new() { }

        public static void ConstraintsTypes<T>() where T : Base, IFaceFirst, IFaceSecond { }

        public static void ConstraintsCombinationClass<T>() where T : class, IFaceFirst, new() { }

        public static void ConstraintsCombinationStruct<T>() where T : struct, IFaceFirst { }

        public static void ConstraintsEnum<T>() where T : Enum { }

        public static void Out(out int first, int second, out int third)
        {
            first = 0;
            third = 0;
        }

        public static void Params(int first, params int[] others) { }
    }

    private abstract class Base { }

    private interface IFaceFirst { }

    private interface IFaceSecond { }
}
