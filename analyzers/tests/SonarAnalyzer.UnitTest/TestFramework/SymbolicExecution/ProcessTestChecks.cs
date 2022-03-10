/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using SonarAnalyzer.SymbolicExecution.Roslyn;
using ProcessFunc = System.Func<SonarAnalyzer.SymbolicExecution.Roslyn.SymbolicContext, SonarAnalyzer.SymbolicExecution.Roslyn.ProgramState[]>;
using ProcessFuncSimple = System.Func<SonarAnalyzer.SymbolicExecution.Roslyn.SymbolicContext, SonarAnalyzer.SymbolicExecution.Roslyn.ProgramState>;

namespace SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution
{
    internal class PreProcessTestCheck : SymbolicCheck
    {
        private readonly ProcessFuncSimple processSingle;
        private readonly ProcessFunc process;

        public PreProcessTestCheck(ProcessFuncSimple processSingle) =>
            this.processSingle = processSingle;

        public PreProcessTestCheck(ProcessFunc process) =>
            this.process = process;

        protected override ProgramState PreProcessSimple(SymbolicContext context) =>
            processSingle is null ? base.PreProcessSimple(context) : processSingle(context);

        public override ProgramState[] PreProcess(SymbolicContext context) =>
            process is null ? base.PreProcess(context) : process(context);
    }

    internal class PostProcessTestCheck : SymbolicCheck
    {
        private readonly ProcessFuncSimple processSingle;
        private readonly ProcessFunc process;

        public PostProcessTestCheck(ProcessFuncSimple processSingle) =>
            this.processSingle = processSingle;

        public PostProcessTestCheck(ProcessFunc process) =>
            this.process = process;

        protected override ProgramState PostProcessSimple(SymbolicContext context) =>
            processSingle is null ? base.PostProcessSimple(context) : processSingle(context);

        public override ProgramState[] PostProcess(SymbolicContext context) =>
            process is null ? base.PostProcess(context) : process(context);
    }
}
