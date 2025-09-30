extern alias common;

using common::SonarAnalyzer.Core.AnalysisContext;
using Microsoft.CodeAnalysis.CSharp;
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

using SonarAnalyzer.CSharp.Core.Syntax.Utilities;
using CS = SonarAnalyzer.CSharp.Core.Extensions.SonarParametrizedAnalysisContextExtensions;
using VB = SonarAnalyzer.VisualBasic.Core.Extensions.SonarParametrizedAnalysisContextExtensions;

namespace SonarAnalyzer.Test.AnalysisContext;

public partial class SonarAnalysisContextTest
{
    [TestMethod]
    [DataRow(SnippetFileName, false)]
    [DataRow(AnotherFileName, true)]
    public void RegisterNodeAction_UnchangedFiles_SonarParametrizedAnalysisContext(string unchangedFileName, bool expected)
    {
        var context = new DummyAnalysisContext(TestContext, unchangedFileName);
        var sonarContext = new SonarAnalysisContext(context, DummyMainDescriptor);
        var sut = new SonarParametrizedAnalysisContext(sonarContext);
        sut.RegisterNodeAction<SyntaxKind>(CSharpGeneratedCodeRecognizer.Instance, context.DelegateAction);
        sonarContext.RegisterCompilationStartAction(sut.ExecutePostponedActions);

        context.AssertDelegateInvoked(expected);
    }

    [TestMethod]
    [DataRow(SnippetFileName, false)]
    [DataRow(AnotherFileName, true)]
    public void RegisterTreeAction_UnchangedFiles_SonarParametrizedAnalysisContext(string unchangedFileName, bool expected)
    {
        var context = new DummyAnalysisContext(TestContext, unchangedFileName);
        var sut = new SonarParametrizedAnalysisContext(new(context, DummyMainDescriptor));
        sut.RegisterTreeAction(CSharpGeneratedCodeRecognizer.Instance, context.DelegateAction);
        sut.ExecutePostponedActions(new(sut, MockCompilationStartAnalysisContext(context)));  // Manual invocation, because SonarParametrizedAnalysisContext stores actions separately

        context.AssertDelegateInvoked(expected);
    }

    [TestMethod]
    public void RegisterTreeAction_Extension_SonarParametrizedAnalysisContext_CS()
    {
        var context = new DummyAnalysisContext(TestContext);
        var self = new SonarParametrizedAnalysisContext(new(context, DummyMainDescriptor));
        CS.RegisterTreeAction(self, context.DelegateAction);
        self.ExecutePostponedActions(new(self, MockCompilationStartAnalysisContext(context)));  // Manual invocation, because SonarParametrizedAnalysisContext stores actions separately

        context.AssertDelegateInvoked(true);
    }

    [TestMethod]
    public void RegisterTreeAction_Extension_SonarParametrizedAnalysisContext_VB()
    {
        var context = new DummyAnalysisContext(TestContext);
        var self = new SonarParametrizedAnalysisContext(new(context, DummyMainDescriptor));
        VB.RegisterTreeAction(self, context.DelegateAction);
        self.ExecutePostponedActions(new(self, MockCompilationStartAnalysisContext(context)));  // Manual invocation, because SonarParametrizedAnalysisContext stores actions separately

        context.AssertDelegateInvoked(true);
    }

    [TestMethod]
    [DataRow(SnippetFileName, false)]
    [DataRow(AnotherFileName, true)]
    public void RegisterSemanticModelAction_UnchangedFiles_SonarParametrizedAnalysisContext(string unchangedFileName, bool expected)
    {
        var context = new DummyAnalysisContext(TestContext, unchangedFileName);
        var sut = new SonarParametrizedAnalysisContext(new(context, DummyMainDescriptor));
        sut.RegisterSemanticModelAction(CSharpGeneratedCodeRecognizer.Instance, context.DelegateAction);
        ExecutePostponedActions(sut, context);

        context.AssertDelegateInvoked(expected);
    }

    [TestMethod]
    public void RegisterSemanticModelAction_Extension_SonarParametrizedAnalysisContext_CS()
    {
        var context = new DummyAnalysisContext(TestContext);
        var self = new SonarParametrizedAnalysisContext(new(context, DummyMainDescriptor));
        CS.RegisterSemanticModelAction(self, context.DelegateAction);
        ExecutePostponedActions(self, context);

        context.AssertDelegateInvoked(true);
    }

    [TestMethod]
    public void RegisterSemanticModelAction_Extension_SonarParametrizedAnalysisContext_VB()
    {
        var context = new DummyAnalysisContext(TestContext);
        var self = new SonarParametrizedAnalysisContext(new(context, DummyMainDescriptor));
        VB.RegisterSemanticModelAction(self, context.DelegateAction);
        ExecutePostponedActions(self, context);

        context.AssertDelegateInvoked(true);
    }

    private static void ExecutePostponedActions(SonarParametrizedAnalysisContext self, DummyAnalysisContext dummyAnalysisContext)
    {
        var sub = Substitute.For<CompilationStartAnalysisContext>(dummyAnalysisContext.Model.Compilation, dummyAnalysisContext.Options, CancellationToken.None);
        sub
            .When(x => x.RegisterSemanticModelAction(Arg.Any<Action<SemanticModelAnalysisContext>>()))
            .Do(x => x.Arg<Action<SemanticModelAnalysisContext>>().Invoke(dummyAnalysisContext.CreateSemanticModelAnalysisContext()));
        self.ExecutePostponedActions(new(self, sub));
    }
}
