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

using SonarAnalyzer.SymbolicExecution.Roslyn;
using ProcessFunc = System.Func<SonarAnalyzer.SymbolicExecution.Roslyn.SymbolicContext, SonarAnalyzer.SymbolicExecution.Roslyn.States<SonarAnalyzer.SymbolicExecution.Roslyn.ProgramState>>;
using ProcessFuncSimple = System.Func<SonarAnalyzer.SymbolicExecution.Roslyn.SymbolicContext, SonarAnalyzer.SymbolicExecution.Roslyn.ProgramState>;
using ProgramStates = SonarAnalyzer.SymbolicExecution.Roslyn.States<SonarAnalyzer.SymbolicExecution.Roslyn.ProgramState>;

namespace SonarAnalyzer.Test.TestFramework.SymbolicExecution
{
    internal class PreProcessTestCheck : ProcessTestCheckBase
    {
        public PreProcessTestCheck(ProcessFuncSimple processSimple) : base(OperationKind.None, processSimple) { }

        public PreProcessTestCheck(OperationKind kind, ProcessFuncSimple processSimple) : base(kind, processSimple) { }

        public PreProcessTestCheck(ProcessFunc process) : base(process) { }

        protected override ProgramState PreProcessSimple(SymbolicContext context) =>
            ProcessSimple(context);

        public override ProgramStates PreProcess(SymbolicContext context) =>
            Process(context);
    }

    internal class PostProcessTestCheck : ProcessTestCheckBase
    {
        public PostProcessTestCheck(ProcessFuncSimple processSimple) : base(OperationKind.None, processSimple) { }

        public PostProcessTestCheck(OperationKind kind, ProcessFuncSimple processSimple) : base(kind, processSimple) { }

        public PostProcessTestCheck(ProcessFunc process) : base(process) { }

        protected override ProgramState PostProcessSimple(SymbolicContext context) =>
            ProcessSimple(context);

        public override ProgramStates PostProcess(SymbolicContext context) =>
            Process(context);
    }

    internal class ProcessTestCheckBase : SymbolicCheck
    {
        private readonly ProcessFuncSimple processSimple;
        private readonly ProcessFunc process;
        private readonly OperationKind kind;

        protected ProcessTestCheckBase(OperationKind kind, ProcessFuncSimple processSimple)
        {
            this.kind = kind;
            this.processSimple = processSimple;
        }

        protected ProcessTestCheckBase(ProcessFunc process) =>
            this.process = process;

        protected ProgramState ProcessSimple(SymbolicContext context) =>
            processSimple is not null && MatchesKind(context) ? processSimple(context) : context.State;

        protected ProgramStates Process(SymbolicContext context)
        {
            if (process is not null && MatchesKind(context))
            {
                return process(context);
            }
            else if (ProcessSimple(context) is { } newState)
            {
                return new(newState);
            }
            else
            {
                return new();
            }
        }

        protected bool MatchesKind(SymbolicContext context) =>
            kind == OperationKind.None || context.Operation.Instance.Kind == kind;
    }

    internal class ConditionEvaluatedTestCheck : SymbolicCheck
    {
        private readonly ProcessFuncSimple conditionEvaluated;

        public ConditionEvaluatedTestCheck(ProcessFuncSimple conditionEvaluated) =>
            this.conditionEvaluated = conditionEvaluated;

        public override ProgramState ConditionEvaluated(SymbolicContext context) =>
            conditionEvaluated(context);
    }
}
