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
public class LocalVariableDeclarationAssignmentShouldBeJoinedTest
{
    private readonly VerifierBuilder verifier = new VerifierBuilder<LocalVariableDeclarationAssignmentShouldBeJoined>();

    [TestMethod]
    public void LocalVariableDeclarationAssignmentShouldBeJoined() =>
        verifier.AddPaths("LocalVariableDeclarationAssignmentShouldBeJoined.cs").Verify();

    [TestMethod]
    public void LocalVariableDeclarationAssignmentShouldBeJoined_CSharp7_9() =>
        verifier.AddPaths("LocalVariableDeclarationAssignmentShouldBeJoined.CSharp7-9.cs")
            .WithOptions(LanguageOptions.Between(LanguageVersion.CSharp7, LanguageVersion.CSharp9))
            .Verify();

#if NET

    [TestMethod]
    public void LocalVariableDeclarationAssignmentShouldBeJoined_Latest() =>
        verifier.AddPaths("LocalVariableDeclarationAssignmentShouldBeJoined.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    public void LocalVariableDeclarationAssignmentShouldBeJoined_TopLevelStatements() =>
        verifier.AddPaths("LocalVariableDeclarationAssignmentShouldBeJoined.TopLevelStatements.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .WithTopLevelStatements()
            .Verify();

#endif
}
