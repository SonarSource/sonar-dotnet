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

using SonarAnalyzer.CSharp.Core.Syntax.Extensions;
using CS = Microsoft.CodeAnalysis.CSharp;
using ExtensionsCS = SonarAnalyzer.CSharp.Core.Syntax.Extensions.ObjectCreationExpressionSyntaxExtensions;
using ExtensionsVB = SonarAnalyzer.VisualBasic.Core.Syntax.Extensions.ObjectCreationExpressionSyntaxExtensions;
using SyntaxCS = Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Test.Extensions
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
            var objectCreation = syntaxTree.First<SyntaxCS.ObjectCreationExpressionSyntax>();

            objectCreation.IsKnownType(KnownType.System_DateTime, compilation.GetSemanticModel(syntaxTree)).Should().Be(expectedResult);
        }

        [TestMethod]
        public void GetObjectCreationTypeIdentifier_Null_CS() =>
            ExtensionsCS.GetObjectCreationTypeIdentifier(null).Should().BeNull();

        [TestMethod]
        public void GetObjectCreationTypeIdentifier_Null_VB() =>
            ExtensionsVB.GetObjectCreationTypeIdentifier(null).Should().BeNull();

        private static CS.CSharpCompilation CreateCompilation(string code) =>
            CS.CSharpCompilation.Create("TempAssembly.dll")
                             .AddSyntaxTrees(CS.CSharpSyntaxTree.ParseText(code))
                             .AddReferences(MetadataReferenceFacade.ProjectDefaultReferences)
                             .WithOptions(new CS.CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
