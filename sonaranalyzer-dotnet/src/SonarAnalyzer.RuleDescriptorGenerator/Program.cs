/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SonarAnalyzer.Common;
using SonarAnalyzer.Utilities;

namespace SonarAnalyzer.RuleDescriptorGenerator
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Write("[AnalyzerLanguage: 'cs' for C#, 'vbnet' for VB.Net]");
                Write("All files will be created by the application");

                return;
            }

            WriteXmlDescriptorFiles("rules.xml", "profile.xml", args[0]);
        }

        private static void Write(string text)
        {
            Console.WriteLine(text);
        }

        private static void WriteXmlDescriptorFiles(string rulePath, string profilePath, string lang)
        {
            var language = AnalyzerLanguage.Parse(lang);

            var genericRuleDetails = RuleDetailBuilder.GetAllRuleDetails(language).ToList();
            var ruleDetails = genericRuleDetails.Select(RuleDetail.Convert).ToList();

            Directory.CreateDirectory(lang);

            WriteRuleDescriptorFile(Path.Combine(lang, rulePath), ruleDetails);
            WriteQualityProfileFile(Path.Combine(lang, profilePath), ruleDetails, language);
        }

        private static void WriteQualityProfileFile(string filePath, IEnumerable<RuleDetail> ruleDetails, AnalyzerLanguage language)
        {
            var root = new QualityProfileRoot(language);
            root.Rules.AddRange(ruleDetails
                .Where(descriptor => descriptor.IsActivatedByDefault)
                .Select(descriptor => new QualityProfileRuleDescriptor(language)
                {
                    Key = descriptor.Key
                }));

            SerializeObjectToFile(filePath, root);
        }

        private static void WriteRuleDescriptorFile(string filePath, IEnumerable<RuleDetail> ruleDetails)
        {
            var root = new RuleDescriptorRoot();
            root.Rules.AddRange(ruleDetails);
            SerializeObjectToFile(filePath, root);
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
                serializer.Serialize(writer, objectToSerialize, new XmlSerializerNamespaces(new[] {XmlQualifiedName.Empty}));
                var ruleXml = Encoding.UTF8.GetString(stream.ToArray());
                File.WriteAllText(filePath, ruleXml);
            }
        }
    }
}
