/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Google.Protobuf;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules
{
    public abstract class UtilityAnalyzerBase : SonarDiagnosticAnalyzer
    {
        internal const string ConfigurationAdditionalFile = "ProjectOutFolderPath.txt";
        internal const string IgnoreHeaderCommentsCSharp = "sonar.cs.ignoreHeaderComments";
        internal const string IgnoreHeaderCommentsVisualBasic = "sonar.vbnet.ignoreHeaderComments";

        protected static readonly ISet<string> FileExtensionWhitelist = new HashSet<string> { ".cs", ".csx", ".vb" };

        protected readonly object parameterReadLock = new object();

        protected bool IsAnalyzerEnabled { get; set; }

        protected string WorkDirectoryBasePath { get; set; }

        protected Dictionary<string, bool> IgnoreHeaderComments { get; } = new Dictionary<string, bool>
            {
                { IgnoreHeaderCommentsCSharp, false },
                { IgnoreHeaderCommentsVisualBasic, false },
            };

        protected void ReadParameters(AnalyzerOptions options, string language)
        {
            var sonarLintAdditionalFile = options.AdditionalFiles
                .FirstOrDefault(f => ParameterLoader.IsSonarLintXml(f.Path));

            var projectOutputAdditionalFile = options.AdditionalFiles
                .FirstOrDefault(IsProjectOutput);

            if (sonarLintAdditionalFile == null ||
                projectOutputAdditionalFile == null)
            {
                return;
            }

            lock (this.parameterReadLock)
            {
                var xml = XDocument.Load(sonarLintAdditionalFile.Path);
                var settings = xml.Descendants("Setting");
                ReadHeaderCommentProperties(settings);
                WorkDirectoryBasePath = File.ReadAllLines(projectOutputAdditionalFile.Path).FirstOrDefault(l => !string.IsNullOrEmpty(l));

                if (!string.IsNullOrEmpty(WorkDirectoryBasePath))
                {
                    var suffix = language == LanguageNames.CSharp
                        ? "cs"
                        : "vbnet";
                    WorkDirectoryBasePath = Path.Combine(WorkDirectoryBasePath, "output-" + suffix);
                    IsAnalyzerEnabled = true;
                }
            }
        }

        private void ReadHeaderCommentProperties(IEnumerable<XElement> settings)
        {
            ReadHeaderCommentProperties(settings, IgnoreHeaderCommentsCSharp);
            ReadHeaderCommentProperties(settings, IgnoreHeaderCommentsVisualBasic);
        }

        private void ReadHeaderCommentProperties(IEnumerable<XElement> settings, string propertyName)
        {
            var propertyStringValue = GetPropertyStringValue(settings, propertyName);
            if (propertyStringValue != null &&
                bool.TryParse(propertyStringValue, out var propertyValue))
            {
                IgnoreHeaderComments[propertyName] = propertyValue;
            }
        }

        private static string GetPropertyStringValue(IEnumerable<XElement> settings, string propName)
        {
            return settings
                .FirstOrDefault(s => s.Element("Key")?.Value == propName)
                ?.Element("Value").Value;
        }

        internal static TextRange GetTextRange(FileLinePositionSpan lineSpan)
        {
            return new TextRange
            {
                StartLine = lineSpan.StartLinePosition.GetLineNumberToReport(),
                EndLine = lineSpan.EndLinePosition.GetLineNumberToReport(),
                StartOffset = lineSpan.StartLinePosition.Character,
                EndOffset = lineSpan.EndLinePosition.Character
            };
        }

        internal static bool IsProjectOutput(AdditionalText file) =>
            ParameterLoader.ConfigurationFilePathMatchesExpected(file.Path, ConfigurationAdditionalFile);
    }

    public abstract class UtilityAnalyzerBase<TMessage> : UtilityAnalyzerBase
        where TMessage : IMessage, new()
    {
        private static readonly object fileWriteLock = new TMessage();

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationAction(
                c =>
                {
                    ReadParameters(c.Options, c.Compilation.Language);

                    if (!IsAnalyzerEnabled)
                    {
                        return;
                    }

                    var messages = new List<TMessage>();

                    foreach (var tree in c.Compilation.SyntaxTrees)
                    {
                        if (ShouldGenerateMetrics(tree) &&
                            (AnalyzeAutogeneratedCode || !GeneratedCodeRecognizer.IsGenerated(tree)))
                        {
                            messages.Add(GetMessage(tree, c.Compilation.GetSemanticModel(tree)));
                        }
                    }

                    if (!messages.Any())
                    {
                        return;
                    }

                    var pathToWrite = Path.Combine(WorkDirectoryBasePath, FileName);
                    lock (fileWriteLock)
                    {
                        // Make sure the folder exists
                        Directory.CreateDirectory(WorkDirectoryBasePath);

                        using (var metricsStream = File.Create(pathToWrite))
                        {
                            foreach (var message in messages)
                            {
                                message.WriteDelimitedTo(metricsStream);
                            }
                        }
                    }
                });
        }

        protected virtual bool AnalyzeAutogeneratedCode => false;

        protected abstract TMessage GetMessage(SyntaxTree syntaxTree, SemanticModel semanticModel);

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected abstract string FileName { get; }

        // TODO: Remove this hardcoded hack as soon as the following ticket is fixed:
        // https://jira.sonarsource.com/browse/MMF-672
        private static bool ShouldGenerateMetrics(SyntaxTree tree) =>
            FileExtensionWhitelist.Contains(Path.GetExtension(tree.FilePath));
    }
}
