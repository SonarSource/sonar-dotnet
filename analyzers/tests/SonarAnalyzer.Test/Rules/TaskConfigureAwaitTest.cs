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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class TaskConfigureAwaitTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<TaskConfigureAwait>();

#if NETFRAMEWORK

    [TestMethod]
    public void TaskConfigureAwait_NetFx() =>
        builder.AddPaths("TaskConfigureAwait.NetFx.cs").Verify();

#else

    [TestMethod]
    public void TaskConfigureAwait_NetCore() =>
        builder.AddPaths("TaskConfigureAwait.NetCore.cs").VerifyNoIssues();

#endif

    [TestMethod]
    public void TaskConfigureAwait_ConsoleApp()
    {
        const string code = @"
using System.Threading.Tasks;

public static class EntryPoint
{
    public async static Task Main() => await Task.Delay(1000); // Compliant
}";
        var projectBuilder = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).AddSnippet(code);
        var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
        var analyzer = new TaskConfigureAwait();
        var compilation = projectBuilder.GetCompilation(null, compilationOptions);

        DiagnosticVerifier.Verify(compilation, analyzer);
    }
}
