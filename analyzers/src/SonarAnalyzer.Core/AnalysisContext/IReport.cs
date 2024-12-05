/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.AnalysisContext;

public interface IReport
{
    ReportingContext CreateReportingContext(Diagnostic diagnostic);
}

public interface ITreeReport : IReport
{
    void ReportIssue(DiagnosticDescriptor rule,
                     Location primaryLocation,
                     IEnumerable<SecondaryLocation> secondaryLocations = null,
                     ImmutableDictionary<string, string> properties = null,
                     params string[] messageArgs);

    [Obsolete("Use another overload of ReportIssue, without calling Diagnostic.Create")]
    void ReportIssue(Diagnostic diagnostic);
}

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
