/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
        internal const string ProjectOutFolderPathFileName = "ProjectOutFolderPath.txt"; // Only for backward compatibility with S4MSB <= 5.0
        internal const string IgnoreHeaderCommentsCS = "sonar.cs.ignoreHeaderComments";
        internal const string IgnoreHeaderCommentsVB = "sonar.vbnet.ignoreHeaderComments";

        protected static readonly ISet<string> FileExtensionWhitelist = new HashSet<string> { ".cs", ".csx", ".vb" };

        protected readonly object parameterReadLock = new object();

        protected bool IsAnalyzerEnabled { get; set; }
        protected string OutPath { get; set; }
        protected virtual bool AnalyzeGeneratedCode { get; set; }

        protected Dictionary<string, bool> IgnoreHeaderComments { get; } = new Dictionary<string, bool>
            {
                { IgnoreHeaderCommentsCS, false },
                { IgnoreHeaderCommentsVB, false },
            };

        internal /* for testing */ static TextRange GetTextRange(FileLinePositionSpan lineSpan) =>
            new TextRange
            {
                StartLine = lineSpan.StartLinePosition.GetLineNumberToReport(),
                EndLine = lineSpan.EndLinePosition.GetLineNumberToReport(),
                StartOffset = lineSpan.StartLinePosition.Character,
                EndOffset = lineSpan.EndLinePosition.Character
            };

        protected void ReadParameters(AnalyzerOptions options, string language)
        {
            var settings = PropertiesHelper.GetSettings(options);
            var projectOutputAdditionalFile = options.AdditionalFiles.FirstOrDefault(IsProjectOutput);

            if (!settings.Any() || projectOutputAdditionalFile == null)
            {
                return;
            }

            lock (parameterReadLock)
            {
                ReadHeaderCommentProperties(settings);
                AnalyzeGeneratedCode = ShouldAnalyzeGeneratedCode();
                OutPath = File.ReadAllLines(projectOutputAdditionalFile.Path).FirstOrDefault(l => !string.IsNullOrEmpty(l));

                if (!string.IsNullOrEmpty(OutPath))
                {
                    OutPath = Path.Combine(OutPath, language == LanguageNames.CSharp ? "output-cs" : "output-vbnet");
                    IsAnalyzerEnabled = true;
                }
            }

            //FIXME: Redundant
            bool ShouldAnalyzeGeneratedCode() =>
                PropertiesHelper.ReadBooleanProperty(
                    settings,
                    language == LanguageNames.CSharp
                        ? PropertiesHelper.AnalyzeGeneratedCodeCS
                        : PropertiesHelper.AnalyzeGeneratedCodeVB);
        }

        private void ReadHeaderCommentProperties(IEnumerable<XElement> settings)
        {
            IgnoreHeaderComments[IgnoreHeaderCommentsCS] = PropertiesHelper.ReadBooleanProperty(settings, IgnoreHeaderCommentsCS);
            IgnoreHeaderComments[IgnoreHeaderCommentsVB] = PropertiesHelper.ReadBooleanProperty(settings, IgnoreHeaderCommentsVB);
        }

        private static bool IsProjectOutput(AdditionalText file) =>
            ParameterLoader.ConfigurationFilePathMatchesExpected(file.Path, ProjectOutFolderPathFileName);
    }

    public abstract class UtilityAnalyzerBase<TMessage> : UtilityAnalyzerBase
        where TMessage : IMessage, new()
    {
        private static readonly object FileWriteLock = new TMessage();

        protected abstract string FileName { get; }
        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract TMessage CreateMessage(SyntaxTree syntaxTree, SemanticModel semanticModel);

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationAction(c =>
                {
                    ReadParameters(c.Options, c.Compilation.Language);
                    if (!IsAnalyzerEnabled)
                    {
                        return;
                    }

                    var messages = c.Compilation.SyntaxTrees
                        .Where(ShouldGenerateMetrics)
                        .Select(x => CreateMessage(x, c.Compilation.GetSemanticModel(x)))
                        .ToArray();
                    if (messages.Any())
                    {
                        lock (FileWriteLock)
                        {
                            // Make sure the folder exists
                            Directory.CreateDirectory(OutPath);
                            using var metricsStream = File.Create(Path.Combine(OutPath, FileName));
                            foreach (var message in messages)
                            {
                                message.WriteDelimitedTo(metricsStream);
                            }
                        }
                    }
                });

        private bool ShouldGenerateMetrics(SyntaxTree tree) =>
            FileExtensionWhitelist.Contains(Path.GetExtension(tree.FilePath))
             && (AnalyzeGeneratedCode || !GeneratedCodeRecognizer.IsGenerated(tree));
    }
}
