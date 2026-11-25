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

using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.Core.Test.ShimLayer.Generated;

[TestClass]
public class GeneratorInitializationContextWrapperTest
{
    [TestMethod]
    public void RegisterForSyntaxNotifications_SyntaxContextReceiverCreator()
    {
        var receiver = new ContextReceiver();
        var generator = new TestGenerator
        {
            Initialize = x => x.RegisterForSyntaxNotifications(() => ISyntaxContextReceiverWrapper.From(receiver))
        };
        Run(generator);
        generator.ContextReceiver.Should().BeSameAs(receiver, "the wrapper returned by our creator was unwrapped for Roslyn");
        receiver.Visited.Should().NotBeEmpty("Roslyn called back into the receiver");
    }

    [TestMethod]
    public void RegisterForSyntaxNotifications_SyntaxReceiverCreator()
    {
        var receiver = new Receiver();
        var generator = new TestGenerator
        {
            Initialize = x => x.RegisterForSyntaxNotifications(() => ISyntaxReceiverWrapper.From(receiver))
        };
        Run(generator);
        generator.Receiver.Should().BeSameAs(receiver, "the wrapper returned by our creator was unwrapped for Roslyn");
        receiver.Visited.Should().NotBeEmpty("Roslyn called back into the receiver");
    }

    private static void Run(TestGenerator generator) =>
        CSharpGeneratorDriver.Create(generator).RunGenerators(CSharpCompilation.Create("Test", [CSharpSyntaxTree.ParseText("class C { }")]));

#pragma warning disable RS1042 // Do not implement ISourceGenerator. We test the ISourceGenerator API.
    private sealed class TestGenerator : ISourceGenerator
    {
        public Action<GeneratorInitializationContextWrapper> Initialize { get; init; }
        public ISyntaxContextReceiver ContextReceiver { get; private set; }
        public ISyntaxReceiver Receiver { get; private set; }

        void ISourceGenerator.Initialize(GeneratorInitializationContext context) =>
            Initialize(GeneratorInitializationContextWrapper.From(context));

        void ISourceGenerator.Execute(GeneratorExecutionContext context)
        {
            ContextReceiver = context.SyntaxContextReceiver;
            Receiver = context.SyntaxReceiver;
        }
    }
#pragma warning restore RS1042

    private sealed class ContextReceiver : ISyntaxContextReceiver
    {
        public List<SyntaxNode> Visited { get; } = [];

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context) =>
            Visited.Add(context.Node);
    }

    private sealed class Receiver : ISyntaxReceiver
    {
        public List<SyntaxNode> Visited { get; } = [];

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode) =>
            Visited.Add(syntaxNode);
    }
}
