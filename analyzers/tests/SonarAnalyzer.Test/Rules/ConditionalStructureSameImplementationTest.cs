/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ConditionalStructureSameImplementationTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ConditionalStructureSameImplementation>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ConditionalStructureSameImplementation>();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void ConditionalStructureSameImplementation_If_CSharp() =>
        builderCS.AddPaths("ConditionalStructureSameImplementation_If.cs").Verify();

    [TestMethod]
    public void ConditionalStructureSameImplementation_Switch_CSharp() =>
        builderCS.AddPaths("ConditionalStructureSameImplementation_Switch.cs").Verify();

    // https://github.com/SonarSource/sonar-dotnet/issues/9637
    [TestMethod]
    [DataRow("nameof(first)", "nameof(first)", false)]
    [DataRow("nameof ( first ) ", "nameof(first)", false)]
    [DataRow("nameof(first)", "nameof(second)", true)]
    [DataRow("first.ToLower()", "nameof(second)", true)]
    [DataRow("nameof(first)", "first.ToLower()", true)]
    public void ConditionalStructureSameImplementation_NameOf_CSharp(string firstExpression, string secondExpression, bool isCompliant)
    {
        var compliantComment = isCompliant ? string.Empty : "// Noncompliant";
        var secondaryComment = isCompliant ? string.Empty : "// Secondary";
        var builder = builderCS.AddSnippet($$"""
            public class NameOfExpressions
            {
                public string Method(string first, string second)
                {
                    if (first.Length == 42)
                    {                                   {{secondaryComment}}
                        var ret = {{firstExpression}};
                        return ret;
                    }
                    else if (second.Length == 42)
                    {                                   {{compliantComment}}
                        var ret = {{secondExpression}};
                        return ret;
                    }
                    return "";
                }
            }
            """);
        if (isCompliant)
        {
            builder.VerifyNoIssues();
        }
        else
        {
            builder.Verify();
        }
    }

    [TestMethod]
    [DataRow("NameOf(first)", "NameOf(first)", false)]
    [DataRow("NameOf ( first ) ", "NameOf(first)", false)]
    [DataRow("NameOf(first)", "NameOf(second)", true)]
    [DataRow("first.ToLower()", "NameOf(second)", true)]
    [DataRow("NameOf(first)", "first.ToLower()", true)]
    public void ConditionalStructureSameImplementation_SymbolCannotBeResolved_VB(string firstExpression, string secondExpression, bool isCompliant)
    {
        var compliantComment = isCompliant ? "' Compliant" : "' Noncompliant";
        var builder = builderVB.AddSnippet($$"""
            Public Class NameOfExpressions
                Public Function Method(first as String, second as String)
                    If first.Length = 42 Then
                        Dim ret = {{firstExpression}}
                        Return ret
                    ElseIf second.Length = 42 Then
                        Dim ret = {{secondExpression}}  {{compliantComment}}
                        Return ret
                    End If
                    Return ""
                End Function
            End Class
            """);
        if (isCompliant)
        {
            builder.VerifyNoIssues();
        }
        else
        {
            builder.Verify();
        }
    }

    [TestMethod]
    public void ConditionalStructureSameImplementation_If_VisualBasic() =>
        builderVB.AddPaths("ConditionalStructureSameImplementation_If.vb").Verify();

    [TestMethod]
    public void ConditionalStructureSameImplementation_Switch_VisualBasic() =>
        builderVB.AddPaths("ConditionalStructureSameImplementation_Switch.vb").Verify();

#if NET
    [TestMethod]
    public void ConditionalStructureSameImplementation_Switch_CSharp_Latest() =>
        builderCS.AddPaths("ConditionalStructureSameImplementation_Switch.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).VerifyNoIssues();

    [TestMethod]
    public void ConditionalStructureSameImplementation_RazorFile_CorrectMessage() =>
        builderCS.AddSnippet(
            """
            @code
            {
                public bool someCondition1 { get; set; }
                public void DoSomething1() { }

                public void Method()
                {
                    if (someCondition1)
                    { // Secondary
                        DoSomething1();
                        DoSomething1();
                    }
                    else
                    { // Noncompliant {{Either merge this branch with the identical one on line 9 or change one of the implementations.}}
                        DoSomething1();
                        DoSomething1();
                    }
                }
            }
            """,
            "SomeRazorFile.razor")
        .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
        .Verify();
#endif
}
