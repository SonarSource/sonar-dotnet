/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.CSharp.Styling.Common.Test;

public static class StylingVerifierBuilder
{
    // This should solve only simple cases. Do not add parametrized overloads to preserve the builder logic.
    public static VerifierBuilder Create<TAnalyzer>() where TAnalyzer : DiagnosticAnalyzer, new() =>
        new VerifierBuilder<TAnalyzer>().WithOptions(LanguageOptions.CSharpLatest);  // We don't use older version on our codebase => we don't need to waste time testing them.

    // This should solve only simple cases. Do not add parametrized overloads.
    public static void Verify<TAnalyzer>() where TAnalyzer : DiagnosticAnalyzer, new() =>
        new VerifierBuilder<TAnalyzer>()
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddPaths(typeof(TAnalyzer).Name + ".cs")
            .Verify();
}
