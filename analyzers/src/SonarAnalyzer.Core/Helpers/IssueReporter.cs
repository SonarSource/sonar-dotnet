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

namespace SonarAnalyzer.Helpers;

public static class IssueReporter
{
    private static readonly ImmutableHashSet<string> ExcludedFromDesignTimeRuleIds = ImmutableHashSet.Create(
        "S1481",
        "S927",
        "S4487",
        "S2696",
        "S2259",
        "S1144",
        "S2325",
        "S1117",
        "S1481",
        "S1871");

    public static void ReportIssueCore(
                    Compilation compilation,
                    Func<DiagnosticDescriptor, bool> hasMatchingScope,
                    Func<Diagnostic, ReportingContext> createReportingContext,
                    DiagnosticDescriptor rule,
                    Location primaryLocation,
                    IEnumerable<SecondaryLocation> secondaryLocations = null,
                    ImmutableDictionary<string, string> properties = null,
                    params string[] messageArgs)
    {
        _ = rule ?? throw new ArgumentNullException(nameof(rule));
        secondaryLocations ??= [];
        properties ??= ImmutableDictionary<string, string>.Empty;

        secondaryLocations = secondaryLocations.Where(x => x.Location.IsValid(compilation)).ToArray();
        properties = properties.AddRange(secondaryLocations.Select((x, index) => new KeyValuePair<string, string>(index.ToString(), x.Message)));

        var diagnostic = Diagnostic.Create(
            descriptor: rule,
            location: primaryLocation,
            additionalLocations: secondaryLocations.Select(x => x.Location),
            properties: properties,
            messageArgs);
        ReportIssueCore(hasMatchingScope, createReportingContext, diagnostic);
    }

    [Obsolete("Use another overload of ReportIssue, without calling Diagnostic.Create")]
    public static void ReportIssueCore(
                    Func<DiagnosticDescriptor, bool> hasMatchingScope,
                    Func<Diagnostic, ReportingContext> createReportingContext,
                    Diagnostic diagnostic)
    {
        if (ShouldRaiseOnRazorFile(ref diagnostic)
            && hasMatchingScope(diagnostic.Descriptor)
            && SonarAnalysisContext.LegacyIsRegisteredActionEnabled(diagnostic.Descriptor, diagnostic.Location?.SourceTree))
        {
            var reportingContext = createReportingContext(diagnostic);
            if (!diagnostic.Location.IsValid(reportingContext.Compilation))
            {
                Debug.Fail("Primary location should be part of the compilation. An AD0001 is raised if this is not the case.");
                return;
            }
            // This is the current way SonarLint will handle how and what to report.
            if (SonarAnalysisContext.ReportDiagnostic is not null)
            {
                Debug.Assert(SonarAnalysisContext.ShouldDiagnosticBeReported is null, "Not expecting SonarLint to set both the old and the new delegates.");
                SonarAnalysisContext.ReportDiagnostic(reportingContext);
                return;
            }
            // Standalone NuGet, Scanner run and SonarLint < 4.0 used with latest NuGet
            if (!VbcHelper.IsTriggeringVbcError(reportingContext.Diagnostic)
                && (SonarAnalysisContext.ShouldDiagnosticBeReported?.Invoke(reportingContext.SyntaxTree, reportingContext.Diagnostic) ?? true))
            {
                reportingContext.ReportDiagnostic(reportingContext.Diagnostic);
            }
        }
    }

    private static bool ShouldRaiseOnRazorFile(ref Diagnostic diagnostic)
    {
        // On design time, we only raise on generated .ide.g.cs files if the diagnostic has a mapped location.
        if (GeneratedCodeRecognizer.IsDesignTimeRazorGeneratedFile(diagnostic.Location.SourceTree))
        {
            return diagnostic.Location.GetMappedLineSpan().HasMappedPath
                && !ExcludedFromDesignTimeRuleIds.Contains(diagnostic.Id);
        }
        // On build time, if the diagnostic has a mapped location, we do the mapping ourselves and raise there.
        else if (GeneratedCodeRecognizer.IsBuildTimeRazorGeneratedFile(diagnostic.Location.SourceTree))
        {
            if (diagnostic.Location.GetMappedLineSpan().HasMappedPath)
            {
                // we want to map the locations of razor files exclusively during compile time, and not design time (IDE).
                // The reason is that for IDE, the correct way to raise issues is directly on the generated files.
                // source: https://github.com/dotnet/razor/issues/9308#issuecomment-1883869224
                diagnostic = MapDiagnostic(diagnostic);
                return true;
            }
            else
            {
                return false;
            }
        }
        // If it is neither a compile nor a design time generated file, it is a normal file.
        else
        {
            return true;
        }
    }

    private static Diagnostic MapDiagnostic(Diagnostic diagnostic)
    {
        var mappedLocation = diagnostic.Location.EnsureMappedLocation();

        var descriptor = new DiagnosticDescriptor(
            diagnostic.Descriptor.Id,
            diagnostic.Descriptor.Title,
            diagnostic.GetMessage(),
            diagnostic.Descriptor.Category,
            diagnostic.Descriptor.DefaultSeverity,
            diagnostic.Descriptor.IsEnabledByDefault,
            diagnostic.Descriptor.Description,
            diagnostic.Descriptor.HelpLinkUri,
            diagnostic.Descriptor.CustomTags.ToArray());

        return Diagnostic.Create(descriptor,
            mappedLocation,
            diagnostic.AdditionalLocations.Select(x => x.EnsureMappedLocation()).ToImmutableList(),
            diagnostic.Properties);
    }
}
