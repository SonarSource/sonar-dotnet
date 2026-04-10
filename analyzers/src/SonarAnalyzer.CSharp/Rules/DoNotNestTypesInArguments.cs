/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotNestTypesInArguments : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S4017";
    private const string MessageFormat = "Refactor this method to remove the nested type argument.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                IEnumerable<ParameterSyntax> parameters = c.Node switch
                {
                    BaseMethodDeclarationSyntax { ParameterList.Parameters.Count: > 0 } method
                        when !ImplementsAbstractClassOrInterface(c, method) => method.ParameterList.Parameters,
                    { } node when node.IsKind(SyntaxKindEx.LocalFunctionStatement) => ((LocalFunctionStatementSyntaxWrapper)node).ParameterList.Parameters,
                    _ => null
                };

                if (parameters is not null)
                {
                    CheckArguments(c, parameters);
                }
            },
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConversionOperatorDeclaration,
            SyntaxKindEx.LocalFunctionStatement);

    private static bool ImplementsAbstractClassOrInterface(SonarSyntaxNodeReportingContext context, BaseMethodDeclarationSyntax method) =>
        context.Model.GetDeclaredSymbol(method) is { } symbol
        && (symbol.GetOverriddenMember() is { IsAbstract: true } || symbol.InterfaceMembers().Any());

    private static bool MaxDepthReached(SyntaxNode parameterSyntax, SemanticModel model)
    {
        var walker = new GenericWalker(2, model);
        walker.SafeVisit(parameterSyntax);
        return walker.IsMaxDepthReached;
    }

    private static void CheckArguments(SonarSyntaxNodeReportingContext context, IEnumerable<ParameterSyntax> arguments)
    {
        foreach (var argument in arguments.Where(x => MaxDepthReached(x, context.Model)))
        {
            context.ReportIssue(Rule, argument);
        }
    }

    private sealed class GenericWalker : SafeCSharpSyntaxWalker
    {
        private static readonly ImmutableArray<KnownType> IgnoredTypes =
            KnownType.SystemFuncVariants
                .Union(KnownType.SystemActionVariants)
                .Union([KnownType.System_Linq_Expressions_Expression_T])
                .ToImmutableArray();

        private readonly int maxDepth;
        private readonly SemanticModel model;

        private int depth;

        public bool IsMaxDepthReached { get; private set; }

        public GenericWalker(int maxDepth, SemanticModel model)
        {
            this.maxDepth = maxDepth;
            this.model = model;
        }

        public override void VisitGenericName(GenericNameSyntax node)
        {
            if (model.GetSymbolInfo(node).Symbol is not INamedTypeSymbol symbol)
            {
                return;
            }

            if (symbol.ConstructedFrom.IsAny(IgnoredTypes))
            {
                base.VisitGenericName(node);
                return;
            }

            if (depth == maxDepth - 1)
            {
                IsMaxDepthReached = true;
                return;
            }

            depth++;
            base.VisitGenericName(node);
            depth--;
        }
    }
}
