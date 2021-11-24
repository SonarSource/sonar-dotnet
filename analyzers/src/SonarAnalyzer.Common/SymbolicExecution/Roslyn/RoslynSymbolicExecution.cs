﻿/*
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

using System;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    internal class RoslynSymbolicExecution
    {
        private readonly ControlFlowGraph cfg;

        public RoslynSymbolicExecution(ControlFlowGraph cfg) =>
            this.cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));

        public void Execute()
        {
            foreach (var block in cfg.Blocks)    // ToDo: This is a temporary simplification until we support proper branching
            {
                foreach(var operation in block.OperationsAndBranchValue.ToExecutionOrder()) // ToDo: This is a temporary oversimplification to scaffold the engine
                {

                    // ToDo: Something is still missing around here
                }
            }
        }
    }
}
