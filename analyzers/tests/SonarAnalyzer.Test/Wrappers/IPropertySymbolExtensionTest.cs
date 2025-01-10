/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp.Syntax;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.Wrappers;

[TestClass]
public class IPropertySymbolExtensionTest
{
    [DataTestMethod]
    [DataRow("required")]
    [DataRow("")]
    public void IsRequired(string required)
    {
        var net48Attributes = // Compiler attributes to simulate "this compiler/framework combination supports required members"
#if NETFRAMEWORK
            $$"""
            namespace System.Runtime.CompilerServices;
            public class RequiredMemberAttribute : Attribute { }
            public class CompilerFeatureRequiredAttribute(string featureName) : Attribute { }
            """;
#else
            string.Empty;
#endif
        var code = $$"""
            {{net48Attributes}}
            public class Test
            {
                public {{required}} int Prop { get; set; }
            }
            """;
        var (tree, model) = TestCompiler.CompileCS(code);
        var propertyDeclaration = tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>().Single();
        var propertySymbol = model.GetDeclaredSymbol(propertyDeclaration).Should().BeAssignableTo<IPropertySymbol>().Subject;
        propertySymbol.IsRequired().Should().Be(propertySymbol.IsRequired);
    }
}
