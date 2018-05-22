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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Protobuf;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf.Ucfg;
using SonarAnalyzer.Security.Ucfg;
using SonarAnalyzer.SymbolicExecution.ControlFlowGraph;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReviewSqlQueriesForSecurityVulnerabilities : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S9999-ucfg-generator";
        private const string Title = "UCFG generator.";

        private static readonly DiagnosticDescriptor rule = DiagnosticDescriptorBuilder
            .GetUtilityDescriptor(DiagnosticId, Title);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private string protobufDirectory;
        private int protobufFileIndex = 0;

        /// <summary>
        /// Contains the build ID as set by Scanner for MSBuild. Usually it is a number.
        /// We include this in the protobuf file names because the Sonar Security plugin
        /// is unable to read files from subfolders.
        /// </summary>
        private string projectBuildId;

        private IAnalyzerConfiguration configuration;

        public ReviewSqlQueriesForSecurityVulnerabilities()
            : this(new DefaultAnalyzerConfiguration())
        {
        }

        public ReviewSqlQueriesForSecurityVulnerabilities(IAnalyzerConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                cc =>
                {
                    if (!TryReadConfiguration(cc.Options))
                    {
                        return;
                    }

                    protobufFileIndex = 0;

                    cc.RegisterSyntaxNodeActionInNonGenerated(
                        c => WriteUCFG<BaseMethodDeclarationSyntax>(c, x => x.Body),
                        SyntaxKind.ConstructorDeclaration,
                        SyntaxKind.OperatorDeclaration);

                    cc.RegisterSyntaxNodeActionInNonGenerated(
                        c => WriteUCFG<MethodDeclarationSyntax>(c, x => (CSharpSyntaxNode)x.Body ?? x.ExpressionBody?.Expression),
                        SyntaxKind.MethodDeclaration);

                    cc.RegisterSyntaxNodeActionInNonGenerated(
                        c => WriteUCFG<AccessorDeclarationSyntax>(c, node => node.Body),
                        SyntaxKind.GetAccessorDeclaration,
                        SyntaxKind.SetAccessorDeclaration);

                    cc.RegisterSyntaxNodeActionInNonGenerated(
                        c => WriteUCFG<PropertyDeclarationSyntax>(c, node => node.ExpressionBody?.Expression),
                        SyntaxKind.PropertyDeclaration);
                });
        }

        internal /*for testing*/ static bool IsValid(UCFG ucfg)
        {
            var existingBlockIds = new HashSet<string>(ucfg.BasicBlocks.Select(b => b.Id));

            return ucfg.BasicBlocks.All(HasTerminator)
                && ucfg.BasicBlocks.All(JumpsToExistingBlock)
                && ucfg.Entries.All(existingBlockIds.Contains);

            bool HasTerminator(BasicBlock block) =>
                block.Jump != null || block.Ret != null;

            bool JumpsToExistingBlock(BasicBlock block) =>
                block.Jump == null || block.Jump.Destinations.All(existingBlockIds.Contains);
        }

        private void WriteUCFG<TDeclarationSyntax>(SyntaxNodeAnalysisContext context, Func<TDeclarationSyntax, CSharpSyntaxNode> getBody)
            where TDeclarationSyntax : SyntaxNode
        {
            var declaration = (TDeclarationSyntax)context.Node;

            var symbol = context.SemanticModel.GetDeclaredSymbol(declaration);

            var methodSymbol = (symbol is IPropertySymbol propertySymbol)
                ? propertySymbol.GetMethod // We are in PropertyDeclarationSyntax
                : symbol as IMethodSymbol; // all other are methods

            if (methodSymbol == null ||
                methodSymbol.IsAbstract ||
                methodSymbol.IsExtern ||
                !CSharpControlFlowGraph.TryGet(getBody(declaration), context.SemanticModel, out var cfg))
            {
                return;
            }

            var ucfg = new UniversalControlFlowGraphBuilder()
                .Build(context.SemanticModel, declaration, methodSymbol, cfg);

            if (!IsValid(ucfg))
            {
                return;
            }

            var path = Path.Combine(protobufDirectory,
                $"ucfg_{projectBuildId}_{Interlocked.Increment(ref protobufFileIndex)}.pb");
            using (var stream = File.Create(path))
            {
                ucfg.WriteTo(stream);
            }
        }

        private bool TryReadConfiguration(AnalyzerOptions options)
        {
            var basePath = configuration.GetProjectOutputPath(options);

            // the current compilation output dir - "<root>/.sonarqube/out/<index>" where index is 0, 1, 2, etc.
            if (basePath != null)
            {
                // "<root>/.sonarqube/out/0" -> "0" etc.
                projectBuildId = Path.GetFileName(basePath);

                // "<root>/.sonarqube/out/0" -> "<root>/.sonarqube/out/ucfg_cs"
                protobufDirectory = Path.Combine(Path.GetDirectoryName(basePath), $"ucfg_{AnalyzerLanguage.CSharp}");

                Directory.CreateDirectory(protobufDirectory);
            }

            return basePath != null;
        }
    }
}
