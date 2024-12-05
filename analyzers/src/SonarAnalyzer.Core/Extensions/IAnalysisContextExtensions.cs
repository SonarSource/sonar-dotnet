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

using static SonarAnalyzer.Analyzers.DiagnosticDescriptorFactory;

namespace SonarAnalyzer.AnalysisContext;

public static class IAnalysisContextExtensions
{
    /// <summary>
    /// Reads the properties from the SonarLint.xml file and caches the result for the scope of this analysis.
    /// </summary>
    public static SonarLintXmlReader SonarLintXml<T>(this T context) where T : IAnalysisContext =>
        context.Options.SonarLintXml(context.AnalysisContext);

    public static bool IsRazorAnalysisEnabled<T>(this T context) where T : IAnalysisContext =>
        context.AnalysisContext.IsRazorAnalysisEnabled(context.Options, context.Compilation);

    public static bool IsTestProject<T>(this T context) where T : IAnalysisContext
    {
        var projectType = context.ProjectConfiguration().ProjectType;
        return projectType == ProjectType.Unknown
            ? context.Compilation.IsTest()              // SonarLint, NuGet or Scanner <= 5.0
            : projectType == ProjectType.Test;          // Scanner >= 5.1 does authoritative decision that we follow
    }

    /// <summary>
    /// Reads configuration from SonarProjectConfig.xml file and caches the result for scope of this analysis.
    /// </summary>
    public static ProjectConfigReader ProjectConfiguration<T>(this T context) where T : IAnalysisContext =>
        context.AnalysisContext.ProjectConfiguration(context.Options);

    public static bool HasMatchingScope<T>(this T context, ImmutableArray<DiagnosticDescriptor> descriptors) where T : IAnalysisContext
    {
        // Performance: Don't use descriptors.Any(HasMatchingScope), the delegate creation allocates too much memory. https://github.com/SonarSource/sonar-dotnet/issues/7438
        foreach (var descriptor in descriptors)
        {
            if (context.HasMatchingScope(descriptor))
            {
                return true;
            }
        }
        return false;
    }

    public static bool HasMatchingScope<T>(this T context, DiagnosticDescriptor descriptor) where T : IAnalysisContext
    {
        // MMF-2297: Test Code as 1st Class Citizen is not ready on server side yet.
        // ScannerRun: Only utility rules and rules with TEST-ONLY scope are executed for test projects for now.
        // SonarLint & Standalone NuGet & Internal styling rules Txxxx: Respect the scope as before.
        return context.IsTestProject()
            ? ContainsTag(TestSourceScopeTag) && !(descriptor.Id.StartsWith("S") && context.ProjectConfiguration().IsScannerRun && ContainsTag(MainSourceScopeTag) && !ContainsTag(UtilityTag))
            : ContainsTag(MainSourceScopeTag);

        bool ContainsTag(string tag) =>
            descriptor.CustomTags.Contains(tag);
    }
}
