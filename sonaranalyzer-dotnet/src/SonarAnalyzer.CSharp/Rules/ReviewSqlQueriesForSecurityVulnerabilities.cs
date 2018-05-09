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
using SonarAnalyzer.Security.Ucfg;
using SonarAnalyzer.SymbolicExecution.ControlFlowGraph;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ReviewSqlQueriesForSecurityVulnerabilities : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3649";
        private const string MessageFormat = "Make sure to sanitize the parameters of this SQL command.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> checkedTypes = new HashSet<KnownType>
        {
            KnownType.System_Data_Odbc_OdbcCommand,
            KnownType.System_Data_Odbc_OdbcDataAdapter,
            KnownType.System_Data_OleDb_OleDbCommand,
            KnownType.System_Data_OleDb_OleDbDataAdapter,
            KnownType.Oracle_ManagedDataAccess_Client_OracleCommand,
            KnownType.Oracle_ManagedDataAccess_Client_OracleDataAdapter,
            KnownType.System_Data_SqlServerCe_SqlCeCommand,
            KnownType.System_Data_SqlServerCe_SqlCeDataAdapter,
            KnownType.System_Data_SqlClient_SqlCommand,
            KnownType.System_Data_SqlClient_SqlDataAdapter
        };

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
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var objectCreation = (ObjectCreationExpressionSyntax)c.Node;

                    if (c.SemanticModel.GetSymbolInfo(objectCreation).Symbol is IMethodSymbol methodSymbol &&
                        methodSymbol.IsConstructor() &&
                        methodSymbol.ContainingType.IsAny(checkedTypes) &&
                        methodSymbol.Parameters.FirstOrDefault()?.Type.Is(KnownType.System_String) == true &&
                        !IsSanitizedQuery(objectCreation.ArgumentList.Arguments[0].Expression, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, objectCreation.Type.GetLocation()));
                    }
                }, SyntaxKind.ObjectCreationExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var assignment = (AssignmentExpressionSyntax)c.Node;

                    if (c.SemanticModel.GetSymbolInfo(assignment.Left).Symbol is IPropertySymbol propertySymbol &&
                        propertySymbol.Name == "CommandText" &&
                        propertySymbol.ContainingType.IsAny(checkedTypes) &&
                        !IsSanitizedQuery(assignment.Right, c.SemanticModel))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, assignment.Left.GetLocation()));
                    }
                }, SyntaxKind.SimpleAssignmentExpression);

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

            var ucfg = new UniversalControlFlowGraphBuilder(context.SemanticModel, declaration, methodSymbol, cfg)
                .Build();

            var path = Path.Combine(protobufDirectory, $"ucfg_{projectBuildId}_{Interlocked.Increment(ref protobufFileIndex)}.pb");
            using (var stream = File.Create(path))
            {
                ucfg.WriteTo(stream);
            }
        }

        private static bool IsSanitizedQuery(ExpressionSyntax expression, SemanticModel model)
        {
            return model.GetConstantValue(expression).HasValue;
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
