/*
 * SonarAnalyzer for .NET
  Copyright (C) 2015-2018 SonarSource SA
  mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
  modify it under the terms of the GNU Lesser General Public
  License as published by the Free Software Foundation; either
  version 3 of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
  Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
  along with this program; if not, write to the Free Software Foundation,
  Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

extern alias csharp;
using System;
using System.Collections.Generic;
using csharp::SonarAnalyzer.Security;
using csharp::SonarAnalyzer.Security.Ucfg;
using csharp::SonarAnalyzer.SymbolicExecution.ControlFlowGraph;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.UnitTest.Security.Ucfg
{
    public abstract class UcfgBuilderTestBase
    {
        protected static void AssertCollection<T>(IList<T> items, params Action<T>[] asserts)
        {
            items.Should().HaveSameCount(asserts);
            for (var i = 0; i < items.Count; i++)
            {
                asserts[i](items[i]);
            }
        }

        protected UCFG GetUcfgForMethod(string code, string methodName)
        {
            (var method, var semanticModel) = TestHelper.Compile(code, Verifier.SystemWebMvcAssembly).GetMethod(methodName);

            var builder = new UniversalControlFlowGraphBuilder();

            return builder.Build(semanticModel, method,
                semanticModel.GetDeclaredSymbol(method), CSharpControlFlowGraph.Create(method.Body, semanticModel));
        }
    }
}
