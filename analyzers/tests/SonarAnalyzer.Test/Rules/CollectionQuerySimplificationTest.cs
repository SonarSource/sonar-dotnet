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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class CollectionQuerySimplificationTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<CollectionQuerySimplification>();

        [TestMethod]
        public void CollectionQuerySimplification() =>
            builder.AddPaths("CollectionQuerySimplification.cs")
                .Verify();
#if NETFRAMEWORK

        [TestMethod]
        public void CollectionQuerySimplification_NetFx() =>
            builder.AddPaths("CollectionQuerySimplification.NetFx.cs")
                .AddReferences(FrameworkMetadataReference.SystemDataLinq)
                .Verify();

#endif

#if NET

        [TestMethod]
        public void CollectionQuerySimplification_CSharp9() =>
            builder.AddPaths("CollectionQuerySimplification.CSharp9.cs")
                .AddReferences(GetReferencesEntityFrameworkNet())
                .WithTopLevelStatements()
                .Verify();

        private static IEnumerable<MetadataReference> GetReferencesEntityFrameworkNet() =>
            Enumerable.Empty<MetadataReference>()
                .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCore("2.2.6"))
                .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational("2.2.6"))
                .Concat(NuGetMetadataReference.EntityFramework("6.2.0"))
                .Concat(NuGetMetadataReference.SystemComponentModelTypeConverter());

#endif
    }
}
