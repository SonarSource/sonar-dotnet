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

using SonarAnalyzer.Core.Syntax.Extensions;
using SonarAnalyzer.Core.Syntax.Utilities;

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class SyntaxTreeExtensionsTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void IsGenerated_On_GeneratedTree() =>
        IsGenerated("""
            namespace Generated
            {
                class MyClass
                {
                    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
                    void M()
                    {
                        ;;;;
                    }
                }
            }
            """,
            CSharpGeneratedCodeRecognizer.Instance).Should().BeTrue();

    [TestMethod]
    public void IsGenerated_On_GeneratedLocalFunctionTree() =>
        IsGenerated("""
            namespace Generated
            {
                class MyClass
                {
                    void M()
                    {
                        ;;;;
                        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
                        void LocalFunction()
                        {
                            ;;;
                        }
                    }
                }
            }
            """,
            CSharpGeneratedCodeRecognizer.Instance).Should().BeTrue();

    [TestMethod]
    public void IsGenerated_On_NonGeneratedTree() =>
        IsGenerated("""
            namespace NonGenerated
            {
                class MyClass
                {
                }
            }
            """,
            CSharpGeneratedCodeRecognizer.Instance).Should().BeFalse();

    // A source generator maps its generated code back to the original source file via:
    // #pragma checksum (the Razor impl half, *_razor.g.cs)
    // since .NET 11 - C# 10 line-span directives (the Razor decl half, *_razor.decl.g.cs)
    // the classic #line <n> "file" form (used by generators across the wider .NET ecosystem)
    [TestMethod]
    [DataRow(@"#pragma checksum ""C:\Component.razor"" ""{8829d00f-11b8-4213-878b-770e8597ac16}"" ""35c3e85c77eb4f50f311a96f96be44f36c36b570ce2579ec311010076f7ac44d""", "Pages_Component_razor.g.cs")]
    [DataRow(@"#line (1,1)-(1,1) ""C:\Component.razor""", "Pages_Component_razor.decl.g.cs")]
    [DataRow(@"#line 5 ""C:\Component.razor""", "Generated.g.cs")]
    public void OriginalFilePath_WithDirective_ReturnsOriginalRazorPath(string content, string fileName) =>
        CSharpSyntaxTree.ParseText(content, path: fileName, cancellationToken: TestContext.CancellationToken).OriginalFilePath.Should().Be(@"C:\Component.razor");

    [TestMethod]
    [DataRow("namespace NoDirective { }")]
    [DataRow("#line 200")]
    public void OriginalFilePath_WithoutFileDirective_ReturnsTreeFilePath(string content)
    {
        var tree = CSharpSyntaxTree.ParseText(content, path: "Regular.cs", cancellationToken: TestContext.CancellationToken);
        tree.OriginalFilePath.Should().Be(tree.FilePath);
    }

    private bool IsGenerated(string content, GeneratedCodeRecognizer generatedCodeRecognizer) =>
        CSharpSyntaxTree.ParseText(content, path: "File.cs", cancellationToken: TestContext.CancellationToken).IsGenerated(generatedCodeRecognizer);
}
