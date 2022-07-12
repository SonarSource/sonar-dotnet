/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
#if NETCOREAPP      // ToDo: Remove conditional compilation everywhere after change of target framework
using System.Text.Json;
#endif
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Utilities;

namespace SonarAnalyzer.RuleDescriptorGenerator
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                var language = args[0] switch
                {
                    "cs" => AnalyzerLanguage.CSharp,
                    "vbnet" => AnalyzerLanguage.VisualBasic,
                    _ => throw new NotSupportedException("Unsupported argument: " + args[0]),
                };
                var root = new RuleDescriptorRoot(RuleDetailBuilder.GetAllRuleDetails(language).Select(RuleDetail.Convert));
                Directory.CreateDirectory(args[0]);
                SerializeObjectToFile(Path.Combine(args[0], "rules.xml"), root);
            }
#if NETCOREAPP
            else if (args.Length == 2)
            {
                var analyzers = LoadAnalyzerTypes(args[0]);
                var rules = LoadRules(analyzers);
                var json = JsonSerializer.Serialize(rules, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                File.WriteAllText(args[1], json);
            }
#endif
            else
            {
                // ToDo: Will be outdated after removing rules.xml
                Console.WriteLine("[AnalyzerLanguage: 'cs' for C#, 'vbnet' for VB.Net]");
                Console.WriteLine("All files will be created by the application");
            }
        }

        private static void SerializeObjectToFile(string filePath, object objectToSerialize)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                IndentChars = "  "
            };

            using (var stream = new MemoryStream())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                var serializer = new XmlSerializer(objectToSerialize.GetType());
                serializer.Serialize(writer, objectToSerialize, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                var ruleXml = Encoding.UTF8.GetString(stream.ToArray());
                File.WriteAllText(filePath, ruleXml);
            }
        }

#if NETCOREAPP

        private static Type[] LoadAnalyzerTypes(string path) =>
            Assembly.LoadFrom(path)  // We don't need 'Load', we have full control over the environment. It would make local build too complicated.
                .ExportedTypes
                .Where(x => !x.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(x))
                .ToArray();

        private static Rule[] LoadRules(Type[] analyzers) =>
            analyzers
                .Select(x => new { Type = x, Parameters = RuleParameters(x) })
                .SelectMany(x => UniqueIds(x.Type).Select(id => new { Id = id, x.Parameters }))
                .GroupBy(x => x.Id)     // Same id can be in multiple classes (see InvalidCastToInterface)
                .Select(x => new Rule(x.Key, x.SelectMany(x => x.Parameters).ToArray()))
                .ToArray();

        private static RuleParameter[] RuleParameters(Type analyzer) =>
            analyzer.GetProperties()
                .Select(x => x.GetCustomAttributes<RuleParameterAttribute>().SingleOrDefault())
                .Where(x => x != null)
                .Select(x => new RuleParameter(x))
                .ToArray();

        // One class can have the same ruleId multiple times, see S3240
        private static string[] UniqueIds(Type analyzer) =>
            ((DiagnosticAnalyzer)Activator.CreateInstance(analyzer)).SupportedDiagnostics.Select(x => x.Id).Distinct().ToArray();

#endif

    }
}
