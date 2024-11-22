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

using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions
{
    [TestClass]
    public class ObjectCreationExpressionSyntaxExtensionsTest
    {
        [TestMethod]
        [DataRow("new System.DateTime()", true)]
        [DataRow("using System; class T { DateTime field = new DateTime(); }", true)]
        [DataRow("new double()", false)]
        [DataRow("using DT = System.DateTime; class T { DT field = new DT(); }", false)]
        public void IsKnownType_ChecksCtorType(string code, bool expectedResult)
        {
            var compilation = CreateCompilation(code);
            var syntaxTree = compilation.SyntaxTrees.First();
            var objectCreation = syntaxTree.First<ObjectCreationExpressionSyntax>();

            objectCreation.IsKnownType(KnownType.System_DateTime, compilation.GetSemanticModel(syntaxTree)).Should().Be(expectedResult);
        }

        [TestMethod]
        public void GetObjectCreationTypeIdentifier_Null_CS() =>
            ObjectCreationExpressionSyntaxExtensions.GetObjectCreationTypeIdentifier(null).Should().BeNull();

        private static CSharpCompilation CreateCompilation(string code) =>
            CSharpCompilation.Create("TempAssembly.dll")
                             .AddSyntaxTrees(CSharpSyntaxTree.ParseText(code))
                             .AddReferences(MetadataReferenceFacade.ProjectDefaultReferences)
                             .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
