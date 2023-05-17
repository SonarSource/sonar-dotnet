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

namespace SonarAnalyzer.UnitTest.Rules;

public partial class TokenTypeAnalyzerTest
{
    [TestMethod]
    public void ClassClassifications() =>
        ClassifierTestHarness.AssertTokenTypes("""
            [k:using] [u:System];
            [k:public] [k:class] [t:Test]
            {
                [c:// SomeComment]
                [k:public] [t:Test]() { }

                [c:/// <summary>
                /// A Prop
                /// </summary>
            ]   [k:int] [u:Prop] { [k:get]; }
                [k:void] [u:Method]<[t:T]>([t:T] [u:t]) [k:where] [t:T]: [k:class], [t:IComparable], [u:System].[u:Collections].[t:IComparer]
                {
                    [k:var] [u:i] = [n:1];
                    var s = [s:"Hello"];
                    [t:T] [u:local] = [k:default];
                }
            [u:}]
            """);

    [TestMethod]
    public void IndentifierToken_QueryComprehensions() =>
        ClassifierTestHarness.AssertTokenTypes("""
            using System.Linq;
            public class Test {
                public void M()
                {
                    _ = from [u:i] in new int[0]
                        let [u:j] = i
                        join [u:k] in new int[0] on i equals k into [u:l]
                        select l into [u:m]
                        select m;
                }
            }
            """, false);

    [TestMethod]
    public void IndentifierToken_VariableDeclarator() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class Test {
                int [u:i] = 0, [u:j] = 0;
                public void M()
                {
                    int [u:k], [u:l];
                }
            }
            """, false);

    [TestMethod]
    public void IndentifierToken_LabeledStatement() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class Test {
                public void M()
                {
                    goto Label;
            [u:Label]:
                    ;
                }
            }
            """, false);

    [TestMethod]
    public void IndentifierToken_Catch() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class Test {
                public void M()
                {
                    try { }
                    catch(System.Exception [u:ex]) { }
                }
            }
            """, false);

    [TestMethod]
    public void IndentifierToken_ForEach() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class Test {
                public void M()
                {
                    foreach(var [u:i] in new int[0])
                    {
                    }
                }
            }
            """, false);

    [TestMethod]
    public void IndentifierToken_MethodParameterConstructorDestructorLocalFunctionPropertyEvent() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class Test {
                public int [u:Prop] { get; set; }
                public event System.EventHandler [u:TestEventField];
                public event System.EventHandler [u:TestEventDeclaration] { add { } remove { } }
                public [t:Test]() { }
                public void [u:Method]<[t:T]>(int [u:parameter]) { }
                ~[t:Test]()
                {
                    void [u:LocalFunction]() { }
                }
            }
            """, false);

    [TestMethod]
    public void IndentifierToken_BaseTypeDelegateEnumMember() =>
        ClassifierTestHarness.AssertTokenTypes("""
            public class [t:TestClass] { }
            public struct [t:TestStruct] { }
            public record [t:TestRecord] { }
            public record struct [t:TestRecordStruct] { }
            public delegate void [t:TestDelegate]();
            public enum [t:TestEnum] { [u:EnumMember] }
            """, false);

    [TestMethod]
    public void IndentifierToken_TupleDesignation() =>
        ClassifierTestHarness.AssertTokenTypes("""
            class Test
            {
                void M()
                {
                    var ([u:i], [u:j]) = (1, 2);
                    (int [u:i], int [u:j]) [u:t];
                }
            }
            """, false);

    [TestMethod]
    public void IndentifierToken_FunctionPointerUnmanagedCallingConvention() =>
        ClassifierTestHarness.AssertTokenTypes("""
            unsafe class Test
            {
                void M(delegate* unmanaged[[u:Cdecl]]<int, int> m) { }
            }
            """, false);

    [TestMethod]
    public void IndentifierToken_ExternAlias() =>
    ClassifierTestHarness.AssertTokenTypes("""
            extern alias [u:ThisIsAnAlias];
            public class Test {
            }
            """, false, true);

    [TestMethod]
    public void IndentifierToken_AccessorDeclaration() =>
    ClassifierTestHarness.AssertTokenTypes("""
            public class Test {
                public string Property { [u:unknown]; }
            }
            """, false, true);
}
