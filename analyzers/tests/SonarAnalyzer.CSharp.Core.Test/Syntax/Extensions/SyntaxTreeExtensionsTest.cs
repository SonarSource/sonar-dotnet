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
    [DataRow("""
        #line (1,2)-(1,19) "C:\_Imports.razor"
        using System.Text
        #line default
        #line (1,8)-(1,28) "C:\Component.razor"
        #line default
        """,
        "Pages_Component_razor.decl.g.cs")]
    [DataRow("""
        #pragma checksum "C:\_Imports.razor" "{8829d00f-11b8-4213-878b-770e8597ac16}" "35c3e85c77eb4f50f311a96f96be44f36c36b570ce2579ec311010076f7ac44d"
        #line (1,2)-(1,19) "C:\_Imports.razor"
        using System.Text
        #line default
        """,
        "_Imports_razor.g.cs",
        @"C:\_Imports.razor")]
    public void OriginalFilePath_WithDirective_ReturnsOriginalRazorPath(string content, string fileName, string expected = @"C:\Component.razor") =>
        CSharpSyntaxTree.ParseText(content, path: fileName, cancellationToken: TestContext.CancellationToken).OriginalFilePath.Should().Be(expected);

    [TestMethod]
    [DataRow("namespace NoDirective { }")]
    [DataRow("#line 200")]
    [DataRow(@"#line (1,2)-(1,19) ""C:\_Imports.razor""", "Pages_Component_razor.decl.g.cs")] // The decl file of a component without code only has the @using directives inherited from _Imports.razor
    public void OriginalFilePath_WithoutFileDirective_ReturnsTreeFilePath(string content, string fileName = "Regular.cs")
    {
        var tree = CSharpSyntaxTree.ParseText(content, path: fileName, cancellationToken: TestContext.CancellationToken);
        tree.OriginalFilePath.Should().Be(tree.FilePath);
    }

    [TestMethod]
    public void OriginalFilePath_ComponentNameEndingWithImports_ReturnsComponentPath() =>
        CSharpSyntaxTree.ParseText(@"#line (1,1)-(1,1) ""C:\My_Imports.razor""", path: "My_Imports_razor.decl.g.cs", cancellationToken: TestContext.CancellationToken)
            .OriginalFilePath
            .Should().Be(@"C:\My_Imports.razor");

    private bool IsGenerated(string content, GeneratedCodeRecognizer generatedCodeRecognizer) =>
        CSharpSyntaxTree.ParseText(content, path: "File.cs", cancellationToken: TestContext.CancellationToken).IsGenerated(generatedCodeRecognizer);
}
