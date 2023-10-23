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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RedundantArgument : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3254";
        private const string MessageFormat = "Remove this default value assigned to parameter '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    if (ArgumentList(c.Node) is { Arguments.Count: > 0 } argumentList
                        && !IsRedundantPrimaryConstructorBaseTypeContext(c) // FIXME: Use extension method from #8238
                        && !c.IsInExpressionTree() // Can't use optional arguments in expression trees (CS0584), so skip those
                        && new CSharpMethodParameterLookup(argumentList, c.SemanticModel) is { MethodSymbol: { } } methodParameterLookup) // FIXME: Replace argumentList with c.Node after #8238
                    {
                        foreach (var argumentMapping in methodParameterLookup.GetAllArgumentParameterMappings().Reverse().Where(x => ArgumentHasDefaultValue(x, c.SemanticModel)))
                        {
                            c.ReportIssue(Diagnostic.Create(Rule, argumentMapping.Node.GetLocation(), argumentMapping.Symbol.Name));
                        }
                    }
                },
                SyntaxKind.InvocationExpression,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKindEx.ImplicitObjectCreationExpression,
                SyntaxKind.BaseConstructorInitializer,
                SyntaxKind.ThisConstructorInitializer,
                SyntaxKindEx.PrimaryConstructorBaseType);

        // // FIXME: Copy of https://github.com/SonarSource/sonar-dotnet/pull/8238/files#diff-260dcbe170483d6f19c1ccbd5a4159c909e032540194c6621fd94a2461b5f530R57 and should be deleted after #8238
        private static bool IsRedundantPrimaryConstructorBaseTypeContext(SonarSyntaxNodeReportingContext context) =>
            context is
            {
                Node.RawKind: (int)SyntaxKindEx.PrimaryConstructorBaseType,
                Compilation.Language: LanguageNames.CSharp,
                ContainingSymbol.Kind: SymbolKind.NamedType,
            };

        // FIXME: Should be deleted after #8238
        private static ArgumentListSyntax ArgumentList(SyntaxNode node) =>
            node switch
            {
                InvocationExpressionSyntax invocationExpression => invocationExpression.ArgumentList,
                ObjectCreationExpressionSyntax objectCreationExpression => objectCreationExpression.ArgumentList,
                ConstructorInitializerSyntax constructorInitializer => constructorInitializer.ArgumentList,
                { RawKind: (int)SyntaxKindEx.PrimaryConstructorBaseType } => ((PrimaryConstructorBaseTypeSyntaxWrapper)node).ArgumentList,
                { RawKind: (int)SyntaxKindEx.ImplicitObjectCreationExpression } => ((ImplicitObjectCreationExpressionSyntaxWrapper)node).ArgumentList,
                _ => null,
            };

        internal static bool ArgumentHasDefaultValue(NodeAndSymbol<ArgumentSyntax, IParameterSymbol> argumentMapping, SemanticModel semanticModel)
        {
            var argument = argumentMapping.Node;
            var parameter = argumentMapping.Symbol;

            if (!parameter.HasExplicitDefaultValue)
            {
                return false;
            }

            var defaultValue = parameter.ExplicitDefaultValue;
            var argumentValue = semanticModel.GetConstantValue(argument.Expression);
            return argumentValue.HasValue &&
                object.Equals(argumentValue.Value, defaultValue);
        }
    }
}
