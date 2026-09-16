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

namespace SonarAnalyzer.CFG.Extensions;

public static class BasicBlockWrapperExtensions
{
    extension(BasicBlockWrapper block)
    {
        public ControlFlowRegionWrapper EnclosingNonLocalLifetimeRegion => block.EnclosingRegion.EnclosingNonLocalLifetimeRegion;

        public ImmutableArray<ControlFlowBranchWrapper> Successors
        {
            get
            {
                // since Roslyn does not differentiate between pattern types in CFG, it builds unreachable block for missing
                // pattern match even when discard pattern option is presented. In this case we explicitly exclude this branch
                if (SwitchExpressionArmSyntaxWrapper.IsInstance(block.BranchValue?.Syntax)
                    && DiscardPatternSyntaxWrapper.IsInstance(((SwitchExpressionArmSyntaxWrapper)block.BranchValue.Syntax).Pattern))
                {
                    return block.FallThroughSuccessor.WrappedInstance is null ? ImmutableArray<ControlFlowBranchWrapper>.Empty : ImmutableArray.Create(block.FallThroughSuccessor);
                }
                else
                {
                    return new[] { block.FallThroughSuccessor, block.ConditionalSuccessor }.Where(x => x.WrappedInstance is not null).ToImmutableArray();
                }
            }
        }

        public ImmutableArray<BasicBlockWrapper> SuccessorBlocks => block.Successors.Select(x => x.Destination).Where(x => x.WrappedInstance is not null).ToImmutableArray();

        public ImmutableArray<IOperation> OperationsAndBranchValue => block.BranchValue is null ? block.Operations : block.Operations.Add(block.BranchValue);

        public bool IsEnclosedIn(ControlFlowRegionKind kind)
        {
            var enclosing = kind == ControlFlowRegionKind.LocalLifetime ? block.EnclosingRegion : block.EnclosingNonLocalLifetimeRegion;
            return enclosing.Kind == kind;
        }

        public ControlFlowRegionWrapper? EnclosingRegion(ControlFlowRegionKind kind) =>
            block.EnclosingRegion.EnclosingRegionOrSelf(kind);
    }
}
