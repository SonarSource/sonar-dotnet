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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.Json;
using SonarAnalyzer.Common;
using SonarAnalyzer.Rules;

namespace RuleDescriptorGenerator;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            var analyzers = args.Skip(1).SelectMany(LoadAnalyzerTypes).ToArray();
            var rules = LoadRules(analyzers);
            var json = JsonSerializer.Serialize(rules, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            File.WriteAllText(args[0], json);
        }
        else
        {
            Console.WriteLine("Application expects at least two arguments: <Path-To-Output-Json> <Path-To-Dll> [, <Path-To-Dll> ...] ");
            Console.WriteLine("DiagnosticAnalyzer metadata from <Path-To-Dll> will be serialized to <Path-To-Output-Json>.");
        }
    }

    private static Type[] LoadAnalyzerTypes(string path) =>
        Assembly.LoadFrom(path)  // We don't need 'Load', we have full control over the environment. It would make local build too complicated.
            .ExportedTypes
            .Where(x => !x.IsAbstract
                && typeof(DiagnosticAnalyzer).IsAssignableFrom(x)
                && !IsUtilityAnalyzer(x)
                && !x.Name.Contains('{')) // Due to the merging of assemblies some classes are duplicated with a '{GUID}' suffix, e.g. 'DeadStores{D6DF12A3-12E5-4602-8A69-B80EE29DFD59}'
            .ToArray();

    private static Rule[] LoadRules(Type[] analyzers) =>
        analyzers.Select(x => new { Type = x, Parameters = RuleParameters(x) })
            .SelectMany(x => UniqueIds(x.Type).Select(id => new { Id = id, x.Parameters }))
            .GroupBy(x => x.Id)     // Same id can be in multiple classes (see InvalidCastToInterface)
            .Select(x => new Rule(x.Key, x.SelectMany(x => x.Parameters).ToArray()))
            .ToArray();

    private static RuleParameter[] RuleParameters(Type analyzer) =>
        analyzer.GetProperties()
            .Select(x => x.GetCustomAttributes().SingleOrDefault(x => x.GetType().Name == nameof(RuleParameterAttribute)))
            .Where(x => x is not null)
            .Select(x => new RuleParameter(x))
            .ToArray();

    // One class can have the same ruleId multiple times, see S3240
    private static string[] UniqueIds(Type analyzer) =>
        ((DiagnosticAnalyzer)Activator.CreateInstance(analyzer)).SupportedDiagnostics.Select(x => x.Id).Distinct().ToArray();

    private static bool IsUtilityAnalyzer(Type analyzerType)
    {
        var baseType = analyzerType;
        while (baseType is not null)
        {
            // this needs to be checked by name due to the merging of assemblies in the pipeline
            // UtilityAnalyzerBase is in the SonarAnalyzer.Core assembly
            if (baseType.FullName == typeof(UtilityAnalyzerBase).FullName)
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }
}
