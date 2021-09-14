/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using Microsoft.CodeAnalysis.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IIsNullOperation = Microsoft.CodeAnalysis.FlowAnalysis.IIsNullOperation;

namespace SonarAnalyzer.UnitTest.LiveVariableAnalysis
{
    public partial class RoslynLiveVariableAnalysisTest
    {
        [DataTestMethod]
        [DataRow("using (var ms = new System.IO.MemoryStream()) {", "}")]
        [DataRow("using var ms = new System.IO.MemoryStream();", null)]
        public void Using_LiveInUntilTheEnd(string usingStatement, string suffix)
        {
            /*       Block 1                    Finally region:
             *       ms = new                   Block 4
             *         |                        /    \
             *         |                    Block5    \
             *       Block 2                ms.Dispose |
             *       Method(ms.Length)          \     /
             *        /   \                     Block 6
             *       /     \                      |
             *   Block 3    |                   (null)
             *   Method(0)  |
             *       \     /
             *        Exit
             */
            var code = @$"
{usingStatement}
    Method(ms.Length);
    if (boolParameter)
        Method(0);
{suffix}";
            var context = new Context(code);
            context.Validate(context.Cfg.EntryBlock, new LiveIn("boolParameter"), new LiveOut("boolParameter"));
            context.Validate(context.Block<ISimpleAssignmentOperation>("ms = new System.IO.MemoryStream()"), new LiveIn("boolParameter"), new LiveOut("boolParameter", "ms"));
            context.Validate(context.Block("Method(ms.Length);"), new LiveIn("boolParameter", "ms") /* ToDo: Try/Finally support should introduce new LiveOut("ms") */);
            context.Validate(context.Block("Method(0);") /* ToDo: Try/Finally support should introduce new LiveIn("ms"), new LiveOut("ms") */);
            context.Validate(context.Cfg.ExitBlock);
            // Finally region
            context.Validate(context.Block<IIsNullOperation>("ms = new System.IO.MemoryStream()"), new LiveIn("ms"), new LiveOut("ms"));    // Null check
            context.Validate(context.Block<IInvocationOperation>("ms = new System.IO.MemoryStream()"), new LiveIn("ms"));                   // Actual Dispose
            context.Validate(context.Cfg.Blocks[6]);
        }
    }
}
