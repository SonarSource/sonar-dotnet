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

using SonarAnalyzer.Core.Facade;
using SonarAnalyzer.CSharp.Core.Facade.Implementation;

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Utilities;

[TestClass]
public class TypeDeclarationsTest
{
    [TestMethod]
    public void Of_ReturnsFacadeArray() =>
        TypeDeclarations.Of(TypeDeclarationKinds.ClassLike).Should().BeSameAs(CSharpSyntaxKindFacade.Instance.TypeDeclarations(TypeDeclarationKinds.ClassLike))
            .And.BeEquivalentTo([SyntaxKind.ClassDeclaration, SyntaxKindEx.RecordDeclaration]);
}
