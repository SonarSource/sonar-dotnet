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

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class DiagnosticDescriptorExtensionsTest
{
    [TestMethod]
    public void IsSecurityHotspot_IsHotspot()
    {
        var descriptor = new DiagnosticDescriptor("S2092", "title", "message", "category", DiagnosticSeverity.Warning, true);
        descriptor.IsSecurityHotspot().Should().BeTrue();
    }

    [TestMethod]
    public void IsSecurityHotspot_NonexistentId()
    {
        var descriptor = new DiagnosticDescriptor("Sxxxx", "title", "message", "category", DiagnosticSeverity.Warning, true);
        descriptor.IsSecurityHotspot().Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("S101")] // Both C# and VB rules
    [DataRow("S100")] // C# rule
    [DataRow("S117")] // VB rule
    public void IsSecurityHotspot_NotHotspot(string ruleId)
    {
        var descriptor = new DiagnosticDescriptor(ruleId, "title", "message", "category", DiagnosticSeverity.Warning, true);
        descriptor.IsSecurityHotspot().Should().BeFalse();
    }
}
