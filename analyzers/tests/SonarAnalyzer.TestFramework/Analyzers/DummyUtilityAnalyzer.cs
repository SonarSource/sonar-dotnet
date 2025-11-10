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

using Google.Protobuf;
using SonarAnalyzer.Core.AnalysisContext;
using SonarAnalyzer.Core.Rules;

namespace SonarAnalyzer.TestFramework.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class DummyUtilityAnalyzerCS : DummyUtilityAnalyzer
{
    public DummyUtilityAnalyzerCS(string protobufPath, IMessage message) : base(protobufPath, message) { }
}

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
internal class DummyUtilityAnalyzerVB : DummyUtilityAnalyzer
{
    public DummyUtilityAnalyzerVB(string protobufPath, IMessage message) : base(protobufPath, message) { }
}

internal abstract class DummyUtilityAnalyzer : UtilityAnalyzerBase
{
    private readonly string protobufPath;
    private readonly IMessage message;

    protected DummyUtilityAnalyzer(string protobufPath, IMessage message) : base("SDummyUtility", "Dummy title")
    {
        this.protobufPath = protobufPath;
        this.message = message;
    }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationAction(c =>
        {
            using var output = File.Create(protobufPath);
            message?.WriteDelimitedTo(output);
        });
}
