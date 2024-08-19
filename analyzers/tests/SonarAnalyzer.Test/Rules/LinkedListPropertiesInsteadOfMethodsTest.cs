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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class LinkedListPropertiesInsteadOfMethodsTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.LinkedListPropertiesInsteadOfMethods>();

    [TestMethod]
    public void LinkedListPropertiesInsteadOfMethods_CS() =>
        builderCS.AddPaths("LinkedListPropertiesInsteadOfMethods.cs").Verify();

    [TestMethod]
    public void LinkedListPropertiesInsteadOfMethods_CS_CodeFix() =>
        builderCS.AddPaths("LinkedListPropertiesInsteadOfMethods.cs")
            .WithCodeFix<CS.LinkedListPropertiesInsteadOfMethodsCodeFix>()
            .WithCodeFixedPaths("LinkedListPropertiesInsteadOfMethods.Fixed.cs")
            .VerifyCodeFix();

#if NET

    [DataTestMethod]
    [DataRow("First")]
    [DataRow("Last")]
    public void LinkedListPropertiesInsteadOfMethods_TopLevelStatements(string name) =>
        builderCS.AddSnippet($$$"""
            using System.Collections.Generic;
            using System.Linq;

            var data = new LinkedList<int>();
            
            data.{{{name}}}(); // Noncompliant {{'{{{name}}}' property of 'LinkedList' should be used instead of the '{{{name}}}()' extension method.}}
            //   {{{new string('^', name.Length)}}}
            
            data.{{{name}}}(x => x > 0);            // Compliant
            _ = data.{{{name}}}.Value;              // Compliant
            data.Count();                           // Compliant
            data.Append(1).{{{name}}}().ToString(); // Compliant
            data?.{{{name}}}().ToString();          // Noncompliant

            """).WithTopLevelStatements().Verify();

#endif

    [TestMethod]
    public void LinkedListPropertiesInsteadOfMethods_VB() =>
        new VerifierBuilder<VB.LinkedListPropertiesInsteadOfMethods>().AddPaths("LinkedListPropertiesInsteadOfMethods.vb").Verify();
}
