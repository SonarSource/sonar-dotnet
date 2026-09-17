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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class LoopsAndLinqTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<LoopsAndLinq>();

    [TestMethod]
    public void LoopsAndLinq_CS() =>
        builder.AddPaths("LoopsAndLinq.cs")
            .AddReferences(MetadataReferenceFacade.SystemData)
            .Verify();

    [TestMethod]
    public void LoopsAndLinq_CS_Latest() =>
        builder.AddPaths("LoopsAndLinq.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    [DataRow("class", "null")]
    [DataRow("class", "default")]
    [DataRow("class", "default(string)")]
    [DataRow("struct", "null")]
    [DataRow("struct", "default")]
    [DataRow("struct", "default(string)")]
    public void LoopsAndLinq_UserDefinedReturnConversion(string elementKind, string fallback) =>
        builder.AddSnippet($$$"""
            using System;
            using System.Collections.Generic;

            {{{elementKind}}} Element
            {
                public static implicit operator string(Element element) => "none";
            }

            class Example
            {
                string Find(IEnumerable<Element> source, Predicate<Element> condition)
                {
                    foreach (var element in source) // Noncompliant {{Loops should be simplified using the "Where" LINQ method}}
                    {
                        if (condition(element))     // Secondary
                            return element;
                    }
                    return {{{fallback}}};
                }
            }
            """)
            .WithOptions(LanguageOptions.Between(LanguageVersion.CSharp7_1, LanguageVersion.CSharp14))
            .Verify();
}
