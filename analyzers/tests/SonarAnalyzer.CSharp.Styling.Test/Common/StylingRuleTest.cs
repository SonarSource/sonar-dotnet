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

using FluentAssertions;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.CSharp.Styling.Common;

namespace SonarAnalyzer.CSharp.Styling.Test.Rules;

[TestClass]
public class StylingRuleTest
{
    private readonly Type[] stylingAnalyzers = typeof(StylingAnalyzer).Assembly.GetExportedTypes().Where(x => typeof(DiagnosticAnalyzer).IsAssignableFrom(x) && !x.IsAbstract).ToArray();

    [TestMethod]
    public void StylingRuleTestScaffolding_FindsAnalyzers() =>
        stylingAnalyzers.Should().NotBeEmpty();

    [TestMethod]
    public void Analyzers_InheritStylingAnalyzer()
    {
        foreach (var type in stylingAnalyzers)
        {
            type.Should().BeAssignableTo(typeof(StylingAnalyzer), "Styling rules should have a simple strucure. StylingAnalyzer should be enough.");
        }
    }

    [TestMethod]
    public void RuleIDs_AreUnique()
    {
        var ids = new Dictionary<string, Type>();
        foreach (var type in stylingAnalyzers)
        {
            var analyzer = (DiagnosticAnalyzer)Activator.CreateInstance(type);
            foreach (var descriptor in analyzer.SupportedDiagnostics)
            {
                ids.TryAdd(descriptor.Id, type).Should().BeTrue("Each rule ID should be registered by a single analyzer but {0} is used by at least the two analyzers {1} and {2}.",
                    descriptor.Id, type, ids[descriptor.Id]);
            }
        }
    }
}
