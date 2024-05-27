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
        var (tree, model) = TestHelper.CompileCS(code);
        var propertyDeclaration = tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>().Single();
        var propertySymbol = model.GetDeclaredSymbol(propertyDeclaration).Should().BeAssignableTo<IPropertySymbol>().Subject;
        propertySymbol.IsRequired().Should().Be(propertySymbol.IsRequired);
    }
}
