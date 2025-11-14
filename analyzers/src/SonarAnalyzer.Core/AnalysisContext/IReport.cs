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

namespace SonarAnalyzer.Core.AnalysisContext;

public interface IReport
{
    ReportingContext CreateReportingContext(Diagnostic diagnostic);
}

/// <summary>
/// Interface for reporting contexts that are executed on a known Tree. The decisions about generated code and unchanged files are taken during action registration.
/// </summary>
public interface ITreeReport : IReport
{
    SyntaxTree Tree { get; }

    void ReportIssue(DiagnosticDescriptor rule,
                     Location primaryLocation,
                     IEnumerable<SecondaryLocation> secondaryLocations = null,
                     ImmutableDictionary<string, string> properties = null,
                     params string[] messageArgs);

    [Obsolete("Use another overload of ReportIssue, without calling Diagnostic.Create")]
    void ReportIssue(Diagnostic diagnostic);
}

/// <summary>
/// Base class for reporting contexts that are common for the entire compilation. Specific tree is not known before the action is executed.
/// </summary>
public interface ICompilationReport : IReport
{
    void ReportIssue(GeneratedCodeRecognizer generatedCodeRecognizer,
                     DiagnosticDescriptor rule,
                     Location primaryLocation,
                     IEnumerable<SecondaryLocation> secondaryLocations = null,
                     params string[] messageArgs);

    [Obsolete("Use another overload of ReportIssue, without calling Diagnostic.Create")]
    void ReportIssue(GeneratedCodeRecognizer generatedCodeRecognizer, Diagnostic diagnostic);
}
