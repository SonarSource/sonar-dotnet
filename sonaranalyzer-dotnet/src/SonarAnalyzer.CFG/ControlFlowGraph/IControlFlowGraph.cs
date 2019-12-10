/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System.Collections.Generic;

namespace SonarAnalyzer.ControlFlowGraph
{
    /// <summary>
    /// Represents a Control Flow Graph of a method or property.
    /// Provides access to the entry, exit and all blocks (<see cref="Block"/>) inside the CFG.
    /// </summary>
    public interface IControlFlowGraph
    {
        IEnumerable<Block> Blocks { get;}

        Block EntryBlock { get; }

        ExitBlock ExitBlock { get; }
    }
}
