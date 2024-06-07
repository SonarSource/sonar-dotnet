/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class InfiniteRecursion : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2190";
    private const string MessageFormat = "Add a way to break out of this {0}.";

    private readonly IChecker checker;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);
    private static DiagnosticDescriptor Rule => DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public InfiniteRecursion() : this(AnalyzerConfiguration.AlwaysEnabled) { }

    internal /* for testing */ InfiniteRecursion(IAnalyzerConfiguration configuration) =>
        checker = configuration.UseSonarCfg() ? new SonarChecker() : new RoslynChecker();

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c =>
            {
                var method = (MethodDeclarationSyntax)c.Node;
                CheckForNoExitMethod(c, method.Identifier);
            },
            SyntaxKind.MethodDeclaration);

        context.RegisterNodeAction(
            c =>
            {
                var function = (LocalFunctionStatementSyntaxWrapper)c.Node;
                CheckForNoExitMethod(c, function.Identifier);
            },
            SyntaxKindEx.LocalFunctionStatement);

        context.RegisterNodeAction(
            c =>
            {
                var @operator = (OperatorDeclarationSyntax)c.Node;
                CheckForNoExitMethod(c, @operator.OperatorToken);
            },
            SyntaxKind.OperatorDeclaration);

        context.RegisterNodeAction(
            c =>
            {
                var conversionOperator = (ConversionOperatorDeclarationSyntax)c.Node;
                CheckForNoExitMethod(c, conversionOperator.OperatorKeyword);
            },
            SyntaxKind.ConversionOperatorDeclaration);

        context.RegisterNodeAction(
            c =>
            {
                var property = (PropertyDeclarationSyntax)c.Node;
                checker.CheckForNoExitProperty(c, property, c.SemanticModel.GetDeclaredSymbol(property));
            },
            SyntaxKind.PropertyDeclaration);

        context.RegisterNodeAction(
            c =>
            {
                var indexer = (IndexerDeclarationSyntax)c.Node;
                checker.CheckForNoExitIndexer(c, indexer, c.SemanticModel.GetDeclaredSymbol(indexer));
            },
            SyntaxKind.IndexerDeclaration);

        context.RegisterNodeAction(
            c =>
            {
                var eventDeclaration = (EventDeclarationSyntax)c.Node;
                checker.CheckForNoExitEvent(c, eventDeclaration, c.SemanticModel.GetDeclaredSymbol(eventDeclaration));
            },
            SyntaxKind.EventDeclaration);
    }

    private void CheckForNoExitMethod(SonarSyntaxNodeReportingContext c, SyntaxToken identifier)
    {
        if (c.SemanticModel.GetDeclaredSymbol(c.Node) is IMethodSymbol symbol)
        {
            checker.CheckForNoExitMethod(c, c.Node, identifier, symbol);
        }
    }

    private static bool IsInstructionOnThisAndMatchesDeclaringSymbol(SyntaxNode node, ISymbol declaringSymbol, SemanticModel semanticModel)
    {
        var name = node is MemberAccessExpressionSyntax memberAccess && memberAccess.Expression.IsKind(SyntaxKind.ThisExpression)
            ? memberAccess.Name
            : node as NameSyntax;

        return name is not null
               && semanticModel.GetSymbolInfo(name).Symbol is { } assignedSymbol
               && declaringSymbol.Equals(assignedSymbol);
    }

    private sealed class RecursionContext<TControlFlowGraph>
    {
        private readonly SonarSyntaxNodeReportingContext analysisContext;
        private readonly string messageArg;
        private readonly Location issueLocation;

        public TControlFlowGraph ControlFlowGraph { get; }
        public ISymbol AnalyzedSymbol { get; }
        public SemanticModel Model => analysisContext.SemanticModel;

        public RecursionContext(SonarSyntaxNodeReportingContext analysisContext,
                                TControlFlowGraph controlFlowGraph,
                                ISymbol analyzedSymbol,
                                Location issueLocation,
                                string messageArg)
        {
            this.analysisContext = analysisContext;
            this.messageArg = messageArg;
            this.issueLocation = issueLocation;
            ControlFlowGraph = controlFlowGraph;
            AnalyzedSymbol = analyzedSymbol;
        }

        public void ReportIssue() =>
            analysisContext.ReportIssue(Rule, issueLocation, messageArg);
    }

    private interface IChecker
    {
        void CheckForNoExitProperty(SonarSyntaxNodeReportingContext c, PropertyDeclarationSyntax property, IPropertySymbol propertySymbol);

        void CheckForNoExitIndexer(SonarSyntaxNodeReportingContext c, IndexerDeclarationSyntax indexer, IPropertySymbol propertySymbol);

        void CheckForNoExitEvent(SonarSyntaxNodeReportingContext c, EventDeclarationSyntax eventDeclaration, IEventSymbol eventSymbol);

        void CheckForNoExitMethod(SonarSyntaxNodeReportingContext c, SyntaxNode body, SyntaxToken identifier, IMethodSymbol symbol);
    }
}
