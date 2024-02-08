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

using System.IO;
using Google.Protobuf;
using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Rules;

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
