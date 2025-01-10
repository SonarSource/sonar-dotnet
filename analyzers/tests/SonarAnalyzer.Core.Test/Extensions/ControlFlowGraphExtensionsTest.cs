/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CFG.Extensions;
using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.Core.Test.Extensions;

[TestClass]
public class ControlFlowGraphExtensionsTest
{
    [TestMethod]
    public void FindLocalFunctionCfgInScope_ThrowForUnexpectedSymbol()
    {
        const string code = @"
public class Sample
{
    public void Method() { }
}";
        var (tree, semanticModel) = TestCompiler.CompileCS(code);
        var method = tree.Single<MethodDeclarationSyntax>();
        var symbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;
        var cfg = ControlFlowGraph.Create(method, semanticModel, default);

        Action a = () => cfg.FindLocalFunctionCfgInScope(symbol, default);
        a.Should().Throw<ArgumentOutOfRangeException>().WithMessage("Specified argument was out of the range of valid values.*Parameter*localFunction*");
    }

    [TestMethod]
    public void FindLocalFunctionCfgInScope_ThrowForNullSymbol()
    {
        const string code = @"
public class Sample
{
    public void Method() { }
}";
        var cfg = TestCompiler.CompileCfgCS(code);
        Action a = () => cfg.FindLocalFunctionCfgInScope(null, default);
        a.Should().Throw<ArgumentOutOfRangeException>().WithMessage("Specified argument was out of the range of valid values.*Parameter*localFunction*");
    }
}
