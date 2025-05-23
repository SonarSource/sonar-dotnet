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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ObsoleteAttributesTest
{
    private readonly VerifierBuilder explanationNeededCS;
    private readonly VerifierBuilder explanationNeededVB;
    private readonly VerifierBuilder removeCS;
    private readonly VerifierBuilder removeVB;

    public ObsoleteAttributesTest()
    {
        var analyzerCs = new CS.ObsoleteAttributes();
        var builderCs = new VerifierBuilder().AddAnalyzer(() => analyzerCs);
        explanationNeededCS = builderCs.WithOnlyDiagnostics(analyzerCs.ExplanationNeededRule);
        removeCS = builderCs.WithOnlyDiagnostics(analyzerCs.RemoveRule);

        var analyzerVb = new VB.ObsoleteAttributes();
        var builderVb = new VerifierBuilder().AddAnalyzer(() => analyzerVb);
        explanationNeededVB = builderVb.WithOnlyDiagnostics(analyzerVb.ExplanationNeededRule);
        removeVB = builderVb.WithOnlyDiagnostics(analyzerVb.RemoveRule);
    }

    [TestMethod]
    public void ObsoleteAttributesNeedExplanation_CS() =>
       explanationNeededCS.AddPaths("ObsoleteAttributesNeedExplanation.cs").Verify();

#if NET
    [TestMethod]
    public void ObsoleteAttributesNeedExplanation_CSharp9() =>
        explanationNeededCS.AddPaths("ObsoleteAttributesNeedExplanation.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void ObsoleteAttributesNeedExplanation_CSharp10() =>
        explanationNeededCS.AddPaths("ObsoleteAttributesNeedExplanation.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

    [TestMethod]
    public void ObsoleteAttributesNeedExplanation_VB14() =>
        explanationNeededVB.AddPaths("ObsoleteAttributesNeedExplanation.VB14.vb").WithOptions(ParseOptionsHelper.FromVisualBasic14).Verify();

#endif

    [TestMethod]
    public void ObsoleteAttributesNeedExplanation_VB() =>
        explanationNeededVB.AddPaths("ObsoleteAttributesNeedExplanation.vb").Verify();

    [TestMethod]
    public void RemoveObsoleteCode_CS() =>
        removeCS.AddPaths("RemoveObsoleteCode.cs").Verify();

    [TestMethod]
    public void RemoveObsoleteCode_CSharp10() =>
        removeCS.AddPaths("RemoveObsoleteCode.CSharp10.cs")
        .WithTopLevelStatements()
        .WithOptions(ParseOptionsHelper.FromCSharp10)
        .Verify();

    [TestMethod]
    public void RemoveObsoleteCode_VB() =>
        removeVB.AddPaths("RemoveObsoleteCode.vb").Verify();

    [DataTestMethod]
    // All attribute targets of [Obsolete]
    [DataRow("bool field;")]                   // AttributeTargets.Field
    [DataRow("event EventHandler SomeEvent;")] // AttributeTargets.Event
    [DataRow("bool Prop { get; set; }")]       // AttributeTargets.Property
    [DataRow("void Method() { }")]             // AttributeTargets.Method
    [DataRow("class C { }")]                   // AttributeTargets.Class
    [DataRow("struct S { }")]                  // AttributeTargets.Struct
    [DataRow("interface I { }")]               // AttributeTargets.Interface
    [DataRow("enum E { A }")]                  // AttributeTargets.Enum
    [DataRow("public Test() { }")]             // AttributeTargets.Constructor
    [DataRow("delegate void Del();")]          // AttributeTargets.Delegate
    [DataRow("int this[int i] => 1;")]         // Indexer
    public void RemoveObsoleteCode_AttributeTargetTest_CS(string attributeTargetDeclaration)
    {
        removeCS.AddSnippet(WrapInTestCode(string.Empty)).VerifyNoIssues();
        removeCS.AddSnippet(WrapInTestCode("[Obsolete] // Noncompliant")).Verify();
        removeCS.AddSnippet(WrapInTestCode("[Custom]")).VerifyNoIssues();
        removeCS.AddSnippet(WrapInTestCode("""
            [Obsolete] // Noncompliant
            [Custom]
            """)).Verify();

        string WrapInTestCode(string attribute) =>
            $$"""
            using System;

            [AttributeUsage(AttributeTargets.All)]
            public sealed class CustomAttribute: Attribute
            {
            }

            public class Test
            {
                {{attribute}}
                {{attributeTargetDeclaration}}
            }
            """;
    }

    [DataTestMethod]
    // All attribute targets of [Obsolete]
    [DataRow("Private field As Boolean")]        // AttributeTargets.Field
    [DataRow("Event SomeEvent As EventHandler")] // AttributeTargets.Event
    [DataRow("Property Prop As Boolean")]        // AttributeTargets.Property
    [DataRow("""
            Private Sub Method()
            End Sub
        """)]                                    // AttributeTargets.Method
    [DataRow("""
            Class C
            End Class
        """)]                                    // AttributeTargets.Class
    [DataRow("""
            Structure S
            End Structure
        """)]                                    // AttributeTargets.Struct
    [DataRow("""
            Interface I
            End Interface
        """)]                                    // AttributeTargets.Interface
    [DataRow("""
            Enum E
                A
            End Enum
        """)]                                    // AttributeTargets.Enum
    [DataRow("""
            Public Sub New()
            End Sub
        """)]                                    // AttributeTargets.Constructor
    [DataRow("Delegate Sub Del()")]              // AttributeTargets.Delegate
    [DataRow("""
            Default ReadOnly Property Item(ByVal i As Integer) As Integer
                Get
                    Return 1
                End Get
            End Property
        """)]                                    // Indexer
    public void RemoveObsoleteCode_AttributeTargetTest_VB(string attributeTargetDeclaration)
    {
        removeVB.AddSnippet(WrapInTestCode(string.Empty)).VerifyNoIssues();
        removeVB.AddSnippet(WrapInTestCode("<Obsolete> ' Noncompliant")).Verify();
        removeVB.AddSnippet(WrapInTestCode("<Custom>")).VerifyNoIssues();
        removeVB.AddSnippet(WrapInTestCode("""
            <Obsolete> ' Noncompliant
            <Custom>
            """)).Verify();

        string WrapInTestCode(string attribute) =>
            $$"""
            Imports System

            <AttributeUsage(AttributeTargets.All)>
            Public NotInheritable Class CustomAttribute
                Inherits Attribute
            End Class

            Public Class Test
                {{attribute}}
                {{attributeTargetDeclaration}}
            End Class
            """;
    }
}
