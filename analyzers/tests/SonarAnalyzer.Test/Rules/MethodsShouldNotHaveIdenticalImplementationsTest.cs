﻿/*
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

    [CombinatorialDataTestMethod]
    public void MethodsShouldNotHaveIdenticalImplementations_MethodTypeParameters(
        [DataValues("", "where T: struct", "where T: class", "where T: unmanaged", "where T: new()", "where T: class, new()")] string constraint1,
        [DataValues("", "where T: struct", "where T: class", "where T: unmanaged", "where T: new()", "where T: class, new()")] string constraint2)
    {
        var nonCompliant = constraint1 == constraint2;
        var builder = builderCS.AddSnippet($$"""
            using System;
            public static class TypeConstraints
            {
                public static bool Compare1<T>(T? value1, T value2) {{constraint1}} {{(nonCompliant ? "// Secondary" : string.Empty)}}
                {
                    Console.WriteLine(value1);
                    Console.WriteLine(value2);
                    return true;
                }

                public static bool Compare2<T>(T? value1, T value2) {{constraint2}} {{(nonCompliant ? "// Noncompliant" : string.Empty)}}
                {
                    Console.WriteLine(value1);
                    Console.WriteLine(value2);
                    return true;
                }
            }
            """).WithOptions(ParseOptionsHelper.FromCSharp9);
        if (nonCompliant)
        {
            builder.Verify();
        }
        else
        {
            builder.VerifyNoIssues();
        }
    }

    [DataTestMethod]
    [DataRow("where T: IEquatable<T>, IComparable", "where T: System.IComparable, IEquatable<T>")]
    [DataRow("where T: List<IEquatable<T>>, IList<T>, IComparable", "where T: List<IEquatable<T>>, IComparable, IList<T>")]
    public void MethodsShouldNotHaveIdenticalImplementations_MethodTypeParameters_NonCompliant(string constraint1, string constraint2) =>
        builderCS.AddSnippet($$"""
            using System;
            using System.Collections.Generic;
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

    [DataTestMethod]
    [DataRow("", "")]
    [DataRow("where TKey: TValue", "where TKey: TValue")]
    [DataRow("where TKey: TValue where TValue: IComparable", "where TKey: TValue where TValue: IComparable")]
    [DataRow("where TKey: IEquatable<TValue> where TValue: IComparable", "where TKey: IEquatable<TValue> where TValue: IComparable")]
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
    [DataRow("Of TKey, TValue", "Of TKey, TValue")]
    [DataRow("Of TKey As Structure, TValue", "Of TKey As Structure, TValue")]
    [DataRow("Of TKey As Structure, TValue As Class", "Of TKey As Structure, TValue As Class")]
    [DataRow("Of TValue As Class, TKey As Structure", "Of TKey As Structure, TValue As Class")]
    [DataRow("Of TKey As {Class}, TValue", "Of TKey As Class, TValue")]
    [DataRow("Of TKey As {New}, TValue", "Of TKey As New, TValue")]
    [DataRow("Of TKey As {IEquatable(Of TKey), IComparable}, TValue", "Of TKey As {System.IComparable, IEquatable(Of TKey)}, TValue")]
    [DataRow("Of TKey As {IEquatable(Of TKey), IComparable}, TValue As IComparable", "Of TValue As IComparable, TKey As {System.IComparable, IEquatable(Of TKey)}")]
    public void MethodsShouldNotHaveIdenticalImplementations_MethodTypeParameters_Dictionary_VB_NonCompliant(string constraint1, string constraint2) =>
        builderVB.AddSnippet($$"""
            Imports System
            Imports System.Collections.Generic

            Class TypeConstraints
                Function Test1({{constraint1}})(dict As IDictionary(Of TKey, TValue)) As Boolean ' Secondary
                    Console.WriteLine(dict)
                    Console.WriteLine(dict)
                    Return True
                End Function

                Function Test2({{constraint2}})(dict As IDictionary(Of TKey, TValue)) As Boolean ' Noncompliant
                    Console.WriteLine(dict)
                    Console.WriteLine(dict)
                    Return True
                End Function
            End Class
            """).Verify();

    [DataTestMethod]
    [DataRow("", "where TKey: struct")]
    [DataRow("where TKey: struct", "")]
    [DataRow("where TKey: struct", "where TKey: class")]
    [DataRow("where TKey: struct where TValue: class", "where TKey: class where TValue: class")]
    [DataRow("where TValue: class where TKey: struct", "where TKey: class where TValue: struct")]
    [DataRow("where TKey: class", "where TKey: class, new()")]
    [DataRow("where TKey: unmanaged", "where TKey: struct")]
    [DataRow("where TKey: new()", "where TKey: IComparable, new()")]
    [DataRow("where TKey: IEquatable<TKey>, IComparable", "where TKey: System.IComparable")]
    [DataRow("where TKey: IEquatable<TKey>, IComparable where TValue: IComparable", " where TKey: System.IComparable where TValue: System.IComparable, IEquatable<TKey>")]
    public void MethodsShouldNotHaveIdenticalImplementations_MethodTypeParameters_Dictionary_Compliant(string constraint1, string constraint2) =>
        builderCS.AddSnippet($$"""
            using System;
            using System.Collections.Generic;
            public static class TypeConstraints
            {
                public static bool Test1<TKey, TValue>(IDictionary<TKey, TValue> dict) {{constraint1}}
                {
                    Console.WriteLine(dict);
                    Console.WriteLine(dict);
                    return true;
                }

                public static bool Test2<TValue, TKey>(IDictionary<TKey, TValue> dict) {{constraint2}}
                {
                    Console.WriteLine(dict);
                    Console.WriteLine(dict);
                    return true;
                }
            }
            """).WithOptions(ParseOptionsHelper.FromCSharp9).VerifyNoIssues();

    [DataTestMethod]
    [DataRow("Of TKey, TValue", "Of TKey, TValue As Structure")]
    [DataRow("Of TKey, TValue As Class", "Of TKey, TValue As Structure")]
    [DataRow("Of TKey As Structure, TValue", "Of TKey, TValue As Structure")]
    [DataRow("Of TKey As {New, IComparable}, TValue", "Of TKey As New, TValue")]
    public void MethodsShouldNotHaveIdenticalImplementations_MethodTypeParameters_Dictionary_VB_Compliant(string constraint1, string constraint2) =>
        builderVB.AddSnippet($$"""
            Imports System
            Imports System.Collections.Generic

            Class TypeConstraints
                Function Test1({{constraint1}})(dict As IDictionary(Of TKey, TValue)) As Boolean
                    Console.WriteLine(dict)
                    Console.WriteLine(dict)
                    Return True
                End Function

                Function Test2({{constraint2}})(dict As IDictionary(Of TKey, TValue)) As Boolean
                    Console.WriteLine(dict)
                    Console.WriteLine(dict)
                    Return True
                End Function
            End Class
            """).VerifyNoIssues();

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
    [DataRow("where TKey: TValue")]
    [DataRow("where TKey: TValue where TValue: IComparable")]
    [DataRow("where TKey: IEquatable<TValue> where TValue: IComparable")]
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

    [DataTestMethod]
    [DataRow("where TSelf: IEqualityOperators<TSelf, TSelf, TResult>")]
    [DataRow("where TSelf: IEqualityOperators<TSelf, TSelf, TResult>, TResult")]
    [DataRow("where TSelf: IEqualityOperators<TSelf, TSelf, TResult> where TResult: IEqualityOperators<TSelf, TSelf, TResult>")]
    [DataRow("where TSelf: IComparisonOperators<TSelf, TSelf, TResult>")]
    [DataRow("where TSelf: IComparisonOperators<TSelf, TSelf, TResult>, TResult")]
    public void MethodsShouldNotHaveIdenticalImplementations_SelfTypes_NonCompliant(string constraint) =>
        builderCS.AddSnippet($$"""
            using System;
            using System.Numerics;
            public class TypeConstraints
            {
                public static bool Test1<TSelf, TResult>(IEqualityOperators<TSelf, TSelf, TResult> x) {{constraint}} // Secondary
                {
                    Console.WriteLine(x);
                    Console.WriteLine(x);
                    return true;
                }

                public static bool Test2<TSelf, TResult>(IEqualityOperators<TSelf, TSelf, TResult> x) {{constraint}}  // Noncompliant
                {
                    Console.WriteLine(x);
                    Console.WriteLine(x);
                    return true;
                }
            }
            """).WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

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
