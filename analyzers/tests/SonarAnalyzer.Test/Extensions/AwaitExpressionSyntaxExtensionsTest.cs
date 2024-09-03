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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.CSharp.Core.Syntax.Extensions;

namespace SonarAnalyzer.Test.Extensions
{
    [TestClass]
    public class AwaitExpressionSyntaxExtensionsTest
    {
        [DataTestMethod]
        [DataRow("[|await t|];", "t")]
        [DataRow("[|await t.ConfigureAwait(false)|];", "t")]
        [DataRow("[|await (t.ConfigureAwait(false))|];", "t")]
        [DataRow("[|await (t).ConfigureAwait(false)|];", "(t)")]
        [DataRow("[|await M(t).ConfigureAwait(false)|];", "M(t)")]
        [DataRow("[|await await TaskOfTask().ConfigureAwait(false)|];", "await TaskOfTask().ConfigureAwait(false)")]
        [DataRow("await [|await TaskOfTask().ConfigureAwait(false)|];", "TaskOfTask()")]
        [DataRow("[|await (await TaskOfTask()).ConfigureAwait(false)|];", "(await TaskOfTask())")]
        public void AwaitedExpressionWithoutConfigureAwait(string expression, string expected)
        {
            var code = $$"""
                using System.Threading.Tasks;
                class C
                {
                    async Task M(Task t)
                    {
                        {{expression}}
                    }

                    Task<Task> TaskOfTask() => default;
                }
                """;
            var start = code.IndexOf("[|");
            code = code.Replace("[|", string.Empty);
            var end = code.IndexOf("|]");
            code = code.Replace("|]", string.Empty);
            var root = CSharpSyntaxTree.ParseText(code).GetRoot();
            var node = root.FindNode(TextSpan.FromBounds(start, end)) as AwaitExpressionSyntax;
            var actual = node.AwaitedExpressionWithoutConfigureAwait();
            actual.ToString().Should().Be(expected);
        }
    }
}
