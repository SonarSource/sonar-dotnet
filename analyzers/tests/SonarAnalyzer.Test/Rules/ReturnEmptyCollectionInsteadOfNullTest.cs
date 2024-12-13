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
public class ReturnEmptyCollectionInsteadOfNullTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ReturnEmptyCollectionInsteadOfNull>();

    [TestMethod]
    public void ReturnEmptyCollectionInsteadOfNull() =>
        builder.AddPaths("ReturnEmptyCollectionInsteadOfNull.cs")
            .AddReferences(MetadataReferenceFacade.SystemXml)
            .Verify();

    [TestMethod]
    public void ReturnEmptyCollectionInsteadOfNull_CSharp8() =>
        builder.AddPaths("ReturnEmptyCollectionInsteadOfNull.CSharp8.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

#if NET

    [TestMethod]
    public void ReturnEmptyCollectionInsteadOfNull_Latest() =>
        builder.AddPaths("ReturnEmptyCollectionInsteadOfNull.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithTopLevelStatements()
            .Verify();

#endif

}
