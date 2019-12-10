/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System.Collections.Generic;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.ControlFlowGraph
{
    public  class BlockIdProvider
    {
        private readonly Dictionary<Block, string> _map = new Dictionary<Block, string>();
        private int _counter;

        public string Get(Block cfgBlock) =>
            _map.GetOrAdd(cfgBlock, b => $"{_counter++}");
    }
}
