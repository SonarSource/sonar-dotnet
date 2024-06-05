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

using Microsoft.CodeAnalysis.CSharp;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class MethodsShouldNotHaveIdenticalImplementationsTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.MethodsShouldNotHaveIdenticalImplementations>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.MethodsShouldNotHaveIdenticalImplementations>();

    [TestMethod]
    public void MethodsShouldNotHaveIdenticalImplementations() =>
        builderCS.AddPaths("MethodsShouldNotHaveIdenticalImplementations.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

    [DataTestMethod]
    [DataRow("", "")]
    [DataRow("where T: struct", "where T: struct")]
    [DataRow("where T: class", "where T: class")]
    [DataRow("where T: unmanaged", "where T: unmanaged")]
    [DataRow("where T: new()", "where T: new()")]
    [DataRow("where T: IEquatable<T>, IComparable",  "where T: System.IComparable, IEquatable<T>")]
    public void MethodsShouldNotHaveIdenticalImplementations_MethodTypeParameters_NonCompliant(string constraint1, string constraint2) =>
        builderCS.AddSnippet($$"""
            using System;
            public static class TypeConstraints
            {
                public static bool Compare1<T>(T? value1, T value2) {{constraint1}} // Secondary
                {
                    Console.WriteLine(value1);
                    Console.WriteLine(value2);
                    return true;
                }

                public static bool Compare2<T>(T? value1, T value2) {{constraint2}} // Noncompliant
                {
                    Console.WriteLine(value1);
                    Console.WriteLine(value2);
                    return true;
                }
            }
            """).WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

    [CombinatorialData]
    public void MethodsShouldNotHaveIdenticalImplementations_MethodTypeParameters_Compliant(
        [DataValues("where T: struct", "where T: class")] string constraint1,
        [DataValues("", "where T: unmanaged", "where T: new()")] string constraint2) =>
        builderCS.AddSnippet($$"""
            using System;
            public static class TypeConstraints
            {
                public static bool Compare1<T>(T? value1, T value2) {{constraint1}}
                {
                    Console.WriteLine(value1);
                    Console.WriteLine(value2);
                    return true;
                }

                public static bool Compare2<T>(T? value1, T value2) {{constraint2}}
                {
                    Console.WriteLine(value1);
                    Console.WriteLine(value2);
                    return true;
                }
            }
            """).WithOptions(ParseOptionsHelper.FromCSharp9).VerifyNoIssues();

    [DataTestMethod]
    [DataRow("", "")]
    [DataRow("where TKey: struct", "where TKey: struct")]
    [DataRow("where TKey: struct where TValue: class", "where TKey: struct where TValue: class")]
    [DataRow("where TValue: class where TKey: struct", "where TKey: struct where TValue: class")]
    [DataRow("where TKey: class", "where TKey: class")]
    [DataRow("where TKey: unmanaged", "where TKey: unmanaged")]
    [DataRow("where TKey: new()", "where TKey: new()")]
    [DataRow("where TKey: IEquatable<TKey>, IComparable", "where TKey: System.IComparable, IEquatable<TKey>")]
    [DataRow("where TKey: IEquatable<TKey>, IComparable where TValue: IComparable", " where TValue: System.IComparable where TKey: System.IComparable, IEquatable<TKey>")]
    public void MethodsShouldNotHaveIdenticalImplementations_MethodTypeParameters_Dictionary_NonCompliant(string constraint1, string constraint2) =>
        builderCS.AddSnippet($$"""
            using System;
            using System.Collections.Generic;
            public static class TypeConstraints
            {
                public static bool Test1<TKey, TValue>(IDictionary<TKey, TValue> dict) {{constraint1}} // Secondary
                {
                    Console.WriteLine(dict);
                    Console.WriteLine(dict);
                    return true;
                }

                public static bool Test2<TValue, TKey>(IDictionary<TKey, TValue> dict) {{constraint2}} // Noncompliant
                {
                    Console.WriteLine(dict);
                    Console.WriteLine(dict);
                    return true;
                }
            }
            """).WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

    [DataTestMethod]
    [DataRow("")]
    [DataRow("where TKey: struct")]
    [DataRow("where TKey: struct where TValue: class")]
    [DataRow("where TValue: class where TKey: struct")]
    [DataRow("where TKey: class")]
    [DataRow("where TKey: unmanaged")]
    [DataRow("where TKey: new()")]
    [DataRow("where TKey: IEquatable<TKey>, IComparable")]
    [DataRow("where TKey: IEquatable<TKey>, IComparable where TValue: IComparable")]
    public void MethodsShouldNotHaveIdenticalImplementations_ClassTypeParameters_Dictionary_NonCompliant(string constraint) =>
        builderCS.AddSnippet($$"""
            using System;
            using System.Collections.Generic;
            public class TypeConstraints<TKey, TValue> {{constraint}}
            {
                public static bool Test1(IDictionary<TKey, TValue> dict) // Secondary
                {
                    Console.WriteLine(dict);
                    Console.WriteLine(dict);
                    return true;
                }

                public static bool Test2(IDictionary<TKey, TValue> dict) // Noncompliant
                {
                    Console.WriteLine(dict);
                    Console.WriteLine(dict);
                    return true;
                }
            }
            """).WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

#if NET

    [TestMethod]
    public void MethodsShouldNotHaveIdenticalImplementations_CSharp9() =>
        builderCS.AddPaths("MethodsShouldNotHaveIdenticalImplementations.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void MethodsShouldNotHaveIdenticalImplementations_CSharp10() =>
        builderCS.AddPaths("MethodsShouldNotHaveIdenticalImplementations.CSharp10.cs").WithLanguageVersion(LanguageVersion.CSharp10).Verify();

    [TestMethod]
    public void MethodsShouldNotHaveIdenticalImplementations_CSharp11() =>
        builderCS.AddPaths("MethodsShouldNotHaveIdenticalImplementations.CSharp11.cs").WithLanguageVersion(LanguageVersion.CSharp11).Verify();

#endif

    [TestMethod]
    public void MethodsShouldNotHaveIdenticalImplementations_VB() =>
        builderVB.AddPaths("MethodsShouldNotHaveIdenticalImplementations.vb").Verify();
}
