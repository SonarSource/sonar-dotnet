/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.ShimLayer.Generator.Model.Test;

[TestClass]
public class ModelBuilderTest
{
    [TestMethod]
    public void Build_NestedTypes()
    {
        var type = typeof(IOperation.OperationList);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], []);
        model.Should().ContainKey(type).And.ContainSingle().Which.Value.Should().BeOfType<SkipStrategy>();
    }

    [TestMethod]
    public void Build_GenericTypes()
    {
        var type = typeof(IEnumerable<int>);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], []);
        model.Should().ContainKey(type).And.ContainSingle().Which.Value.Should().BeOfType<SkipStrategy>();
    }

    [TestMethod]
    public void Build_Delegates()
    {
        var type = typeof(SyntaxReceiverCreator);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], []);
        model.Should().ContainKey(type).And.ContainSingle().Which.Value.Should().BeOfType<SkipStrategy>();
    }

    [TestMethod]
    public void Build_SyntaxNode_Itself()
    {
        var type = typeof(SyntaxNode);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], []);
        model.Should().ContainKey(type).And.ContainSingle().Which.Value.Should().BeOfType<SyntaxNodeStrategy>();
    }

    [TestMethod]
    public void Build_SyntaxNode_SwitchArm()
    {
        var type = typeof(SwitchExpressionArmSyntax);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], []);
        model.Should().ContainKey(type).And.ContainSingle().Which.Value.Should().BeOfType<SyntaxNodeStrategy>();
    }

    [TestMethod]
    public void Build_StaticClass()
    {
        var type = typeof(GeneratorExtensions);
        var model = ModelBuilder.Build([new TypeDescriptor(type, [])], []);
        model.Should().ContainKey(type).And.ContainSingle().Which.Value.Should().BeNull();  // ToDo: This will change later, likely to StaticClassStrategy
    }
}
