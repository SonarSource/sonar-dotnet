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
    public class CollectionsShouldImplementGenericInterfaceTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<CollectionsShouldImplementGenericInterface>()
            .AddReferences(MetadataReferenceFacade.SystemCollections)
            .WithErrorBehavior(CompilationErrorBehavior.Ignore);    // It would be too tedious to implement all those interfaces

        [TestMethod]
        public void CollectionsShouldImplementGenericInterface() =>
            builder.AddPaths("CollectionsShouldImplementGenericInterface.cs").Verify();

#if NET

        [TestMethod]
        public void CollectionsShouldImplementGenericInterface_Csharp9() =>
            builder.AddPaths("CollectionsShouldImplementGenericInterface.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void CollectionsShouldImplementGenericInterface_Csharp10() =>
            builder.AddPaths("CollectionsShouldImplementGenericInterface.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

#endif

    }
}
