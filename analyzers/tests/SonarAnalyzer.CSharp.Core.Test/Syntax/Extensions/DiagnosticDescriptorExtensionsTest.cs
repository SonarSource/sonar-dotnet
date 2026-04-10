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

    [TestMethod]
    [DataRow("S101")] // Both C# and VB rules
    [DataRow("S100")] // C# rule
    [DataRow("S117")] // VB rule
    public void IsSecurityHotspot_NotHotspot(string ruleId)
    {
        var descriptor = new DiagnosticDescriptor(ruleId, "title", "message", "category", DiagnosticSeverity.Warning, true);
        descriptor.IsSecurityHotspot().Should().BeFalse();
    }

    // Repro: https://sonarsource.atlassian.net/browse/NET-3543
    [TestMethod]
    public void IsEnabled_NullSyntaxTreeOptionsProvider_DoesNotThrow()
    {
        var tree = CSharpSyntaxTree.ParseText("class C { void M() {} }", cancellationToken: default);
        var compilation = CSharpCompilation.Create("test", [tree], options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        compilation.Options.SyntaxTreeOptionsProvider.Should().BeNull("this is the precondition that triggers the bug");
        var context = AnalysisScaffolding.CreateNodeReportingContext(tree.GetRoot(default), compilation.GetSemanticModel(tree), _ => { });

        AnalysisScaffolding.CreateDescriptorMain().IsEnabled(context).Should().BeTrue();
    }
}
