/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using SonarAnalyzer.Trackers;

namespace SonarAnalyzer.UnitTest.Trackers;

[TestClass]
public class ArgumentTrackerTest
{
    [TestMethod]
    public void Method_SimpleArgument()
    {
        var snippet = """
            using System;

            public class C
            {
                public void M(IFormatProvider provider)
                {
                    1.ToString($$provider);
                }
            }
            """;
        var pos = snippet.IndexOf("$$");
        snippet = snippet.Replace("$$", string.Empty);
        var (tree, model) = TestHelper.CompileCS(snippet);
        var node = tree.GetRoot().FindNode(new(pos, 0));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "ToString", "provider", 1);
        var match = new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model));
        match.Should().BeTrue();
    }
}
