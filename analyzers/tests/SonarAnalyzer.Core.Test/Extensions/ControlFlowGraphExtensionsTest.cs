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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.Test.Extensions
{
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
            var (tree, semanticModel) = TestHelper.CompileCS(code);
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
            var cfg = TestHelper.CompileCfgCS(code);
            Action a = () => cfg.FindLocalFunctionCfgInScope(null, default);
            a.Should().Throw<ArgumentOutOfRangeException>().WithMessage("Specified argument was out of the range of valid values.*Parameter*localFunction*");
        }
    }
}
