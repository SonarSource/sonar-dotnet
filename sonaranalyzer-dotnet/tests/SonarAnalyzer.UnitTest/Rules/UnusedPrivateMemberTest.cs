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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public partial class UnusedPrivateMemberTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_DebuggerDisplay_Attribute()
        {
            Verifier.VerifyCSharpAnalyzer(@"
// https://github.com/SonarSource/sonar-csharp/issues/1195
[System.Diagnostics.DebuggerDisplay(""{field1}"", Name = ""{Property1} {Property3}"", Type = ""{Method1()}"")]
public class MethodUsages
{
    private int field1;
    private int field2; // Noncompliant
    private int Property1 { get; set; }
    private int Property2 { get; set; } // Noncompliant
    private int Property3 { get; set; }
    private int Method1() { return 0; }
    private int Method2() { return 0; } // Noncompliant

    public void Method()
    {
        var x = Property3;
    }
}
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Members_With_Attributes_Are_Not_Removable()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
public class FieldUsages
{
    [Obsolete]
    private int field1;

    [Obsolete]
    private int Property1 { get; set; }

    [Obsolete]
    private int Method1() { return 0; }

    [Obsolete]
    private class Class1 { }
}
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Assembly_Level_Attributes()
        {
            Verifier.VerifyCSharpAnalyzer(@"
[assembly: System.Reflection.AssemblyCompany(Foo.Constants.AppCompany)]
public static class Foo
{
    internal static class Constants // Compliant, detect usages from assembly level attributes.
    {
        public const string AppCompany = ""foo"";
    }
}
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMemberWithPartialClasses()
        {
            Verifier.VerifyAnalyzer(new[]{
                    @"TestCases\UnusedPrivateMember.part1.cs",
                    @"TestCases\UnusedPrivateMember.part2.cs" },
                new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Methods_EventHandler()
        {
            // Event handler methods are not reported because in WPF an event handler
            // could be added through XAML and no warning will be generated if the
            // method is removed, which could lead to serious problems that are hard
            // to diagnose.
            Verifier.VerifyCSharpAnalyzer(@"
using System;
public class NewClass
{
    private void Handler(object sender, EventArgs e) { } // Noncompliant
}
public partial class PartialClass
{
    private void Handler(object sender, EventArgs e) { } // intentional False Negative
}
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_Unity3D_Ignored()
        {
            Verifier.VerifyCSharpAnalyzer(@"
// https://github.com/SonarSource/sonar-csharp/issues/159
public class UnityMessages1 : UnityEngine.MonoBehaviour
{
    private void SomeMethod(bool hasFocus) { } // Compliant
}

public class UnityMessages2 : UnityEngine.ScriptableObject
{
    private void SomeMethod(bool hasFocus) { } // Compliant
}

public class UnityMessages3 : UnityEditor.AssetPostprocessor
{
    private void SomeMethod(bool hasFocus) { } // Compliant
}

public class UnityMessages4 : UnityEditor.AssetModificationProcessor
{
    private void SomeMethod(bool hasFocus) { } // Compliant
}

// Unity3D does not seem to be available as a nuget package and we cannot use the original classes
namespace UnityEngine
{
    public class MonoBehaviour { }
    public class ScriptableObject { }
}
namespace UnityEditor
{
    public class AssetPostprocessor { }
    public class AssetModificationProcessor { }
}
", new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember()
        {
            Verifier.VerifyAnalyzer(@"TestCases\UnusedPrivateMember.cs",
                new CS.UnusedPrivateMember());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_FromCSharp7()
        {
            Verifier.VerifyAnalyzer(@"TestCases\UnusedPrivateMember.CSharp7.cs",
                new CS.UnusedPrivateMember(),
                ParseOptionsHelper.FromCSharp7);
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void UnusedPrivateMember_CodeFix()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\UnusedPrivateMember.cs",
                @"TestCases\UnusedPrivateMember.Fixed.cs",
                @"TestCases\UnusedPrivateMember.Fixed.Batch.cs",
                new CS.UnusedPrivateMember(),
                new CS.UnusedPrivateMemberCodeFixProvider());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UnusedPrivateMember_UsedInGeneratedFile()
        {
            Verifier.VerifyAnalyzer(
                new[]
                {
                    @"TestCases\UnusedPrivateMember.CalledFromGenerated.cs",
                    @"TestCases\UnusedPrivateMember.Generated.cs"
                },
                new CS.UnusedPrivateMember());
        }
    }
}
