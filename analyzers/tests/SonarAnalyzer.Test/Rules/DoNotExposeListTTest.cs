/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class DoNotExposeListTTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<DoNotExposeListT>();

    [TestMethod]
    public void DoNotExposeListT() =>
        builder.AddPaths("DoNotExposeListT.cs")
            .AddReferences(MetadataReferenceFacade.SystemXml)
            .Verify();

#if NET
    [TestMethod]
    public void DoNotExposeListT_Latest() =>
        builder.AddPaths("DoNotExposeListT.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithTopLevelStatements()
            .Verify();
#endif

    [TestMethod]
    public void DoNotExposeListT_InvalidCode() =>
        builder.AddSnippet("""
            public class InvalidCode
            {
                public List<int> () => null;

                public List<T> { get; set; }

                public List<InvalidType> Method() => null;

                public InvalidType Method2() => null;
            }
            """)
            .VerifyNoIssuesIgnoreErrors();
}
