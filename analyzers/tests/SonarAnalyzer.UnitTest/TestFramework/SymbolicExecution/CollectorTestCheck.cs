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

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution
{
    internal class CollectorTestCheck : SymbolicCheck
    {
        public readonly List<SymbolicContext> PostProcessed = new();
        private readonly List<(string Name, SymbolicContext Context)> tags = new();

        public override ProgramState PostProcess(SymbolicContext context)
        {
            PostProcessed.Add(context);
            if (context.Operation.Instance is IInvocationOperation invocation && invocation.TargetMethod.Name == "Tag")
            {
                var tagName = invocation.Arguments.First().Value.ConstantValue;
                tagName.HasValue.Should().BeTrue("tag should have literal name");
                var name = (string)tagName.Value;
                tags.Any(x => x.Name == name).Should().BeFalse("tags should be unique"); // ToDo: We'll need to redesign this to graph paths for complex branching
                tags.Add((name, context));
            }
            return context.State;
        }

        public void ValidateOrder(params string[] expected) =>
            PostProcessed.Select(x => TestHelper.Serialize(x.Operation)).Should().OnlyContainInOrder(expected);

        public void ValidateTagOrder(params string[] expected) =>
            tags.Select(x => x.Name).Should().BeEquivalentTo(expected);

        public void Validate(string operation, Action<SymbolicContext> action) =>
            action(PostProcessedContext(operation));

        public SymbolicContext PostProcessedContext(string operation) =>
            PostProcessed.Single(x => TestHelper.Serialize(x.Operation) == operation);
    }
}
