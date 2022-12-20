/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.IO;
using Google.Protobuf;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Rules
{
    public abstract class UtilityAnalyzerBase : SonarDiagnosticAnalyzer
    {
        protected static readonly ISet<string> FileExtensionWhitelist = new HashSet<string> { ".cs", ".csx", ".vb" };
        private readonly DiagnosticDescriptor rule;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);
        protected bool IsAnalyzerEnabled { get; set; }
        protected bool IgnoreHeaderComments { get; set; }
        protected virtual bool AnalyzeGeneratedCode { get; set; }
        protected virtual bool AnalyzeTestProjects => true;
        protected string OutPath { get; set; }
        protected bool IsTestProject { get; set; }
        protected override bool EnableConcurrentExecution => false;

        protected UtilityAnalyzerBase(string diagnosticId, string title) =>
            rule = DiagnosticDescriptorFactory.CreateUtility(diagnosticId, title);

        internal static TextRange GetTextRange(FileLinePositionSpan lineSpan) =>
            new()
            {
                StartLine = lineSpan.StartLinePosition.GetLineNumberToReport(),
                EndLine = lineSpan.EndLinePosition.GetLineNumberToReport(),
                StartOffset = lineSpan.StartLinePosition.Character,
                EndOffset = lineSpan.EndLinePosition.Character
            };

        protected void ReadParameters(SonarAnalysisContext context, AnalyzerOptions options, Compilation compilation)
        {
            var settings = PropertiesHelper.GetSettings(options).ToList();
            var outPath = context.ProjectConfiguration(options).OutPath;
            // For backward compatibility with S4MSB <= 5.0
            if (outPath == null && options.ProjectOutFolderPath() is { } projectOutFolderAdditionalFile)
            {
                outPath = projectOutFolderAdditionalFile.GetText().ToString().TrimEnd();
            }
            if (settings.Any() && !string.IsNullOrEmpty(outPath))
            {
                IgnoreHeaderComments = PropertiesHelper.ReadIgnoreHeaderCommentsProperty(settings, compilation.Language);
                AnalyzeGeneratedCode = PropertiesHelper.ReadAnalyzeGeneratedCodeProperty(settings, compilation.Language);
                OutPath = Path.Combine(outPath, compilation.Language == LanguageNames.CSharp ? "output-cs" : "output-vbnet");
                IsAnalyzerEnabled = true;
                IsTestProject = context.IsTestProject(compilation, options);
            }
        }
    }

    public abstract class UtilityAnalyzerBase<TSyntaxKind, TMessage> : UtilityAnalyzerBase
        where TSyntaxKind : struct
        where TMessage : class, IMessage, new()
    {
        private static readonly object FileWriteLock = new TMessage();

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract string FileName { get; }
        protected abstract TMessage CreateMessage(SyntaxTree syntaxTree, SemanticModel semanticModel);

        protected virtual bool AnalyzeUnchangedFiles => false;

        protected virtual IEnumerable<TMessage> CreateAnalysisMessages(Compilation compilation) => Enumerable.Empty<TMessage>();

        protected UtilityAnalyzerBase(string diagnosticId, string title) : base(diagnosticId, title) { }

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(start =>
            {
                ReadParameters(context, start.Options, start.Compilation);
                if (!IsAnalyzerEnabled)
                {
                    return;
                }
                List<TMessage> treeMessages = new();
                start.RegisterSemanticModelAction(model =>
                {
                    ReadParameters(context, model.Options, model.SemanticModel.Compilation);
                    if (!IsAnalyzerEnabled)
                    {
                        return;
                    }
                    var syntaxTree = model.SemanticModel.SyntaxTree;
                    if (ShouldGenerateMetrics(start, syntaxTree))
                    {
                        treeMessages.Add(CreateMessage(syntaxTree, model.SemanticModel));
                    }
                });

                start.RegisterCompilationEndAction(end =>
                {
                    var analysisMessages = CreateAnalysisMessages(end.Compilation).ToList();

                    var allMessages = analysisMessages
                        .Concat(treeMessages)
                        .WhereNotNull()
                        .ToArray();

                    lock (FileWriteLock)
                    {
                        Directory.CreateDirectory(OutPath);
                        using var stream = File.Create(Path.Combine(OutPath, FileName));
                        foreach (var message in allMessages)
                        {
                            message.WriteDelimitedTo(stream);
                        }
                    }
                });
            });

        protected virtual bool ShouldGenerateMetrics(SyntaxTree tree) =>
            // The results of Metrics and CopyPasteToken analyzers are not needed for Test projects yet the plugin side expects the protobuf files, so we create empty ones.
            (AnalyzeTestProjects || !IsTestProject)
            && FileExtensionWhitelist.Contains(Path.GetExtension(tree.FilePath))
            && (AnalyzeGeneratedCode || !Language.GeneratedCodeRecognizer.IsGenerated(tree));

        private bool ShouldGenerateMetrics(CompilationStartAnalysisContext context, SyntaxTree tree) =>
            (AnalyzeUnchangedFiles || !SonarAnalysisContext.IsUnchanged(context.TryGetValue, tree, context.Compilation, context.Options))
            && ShouldGenerateMetrics(tree);
    }
}
