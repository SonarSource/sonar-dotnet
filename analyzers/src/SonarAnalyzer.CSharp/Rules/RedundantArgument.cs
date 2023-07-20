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
                    // Can't use optional arguments in expression trees (CS0584), so skip those
                    if (c.IsInExpressionTree())
                    {
                        return;
                    }

                    var argumentList = c.Node is InvocationExpressionSyntax invocationExpression
                        ? invocationExpression.ArgumentList
                        : ((ImplicitObjectCreationExpressionSyntaxWrapper)c.Node).ArgumentList;
                    var methodParameterLookup = new CSharpMethodParameterLookup(argumentList, c.SemanticModel);

                    if (methodParameterLookup.MethodSymbol != null)
                    {
                        foreach (var argumentMapping in methodParameterLookup.GetAllArgumentParameterMappings().Reverse().Where(x => x.Symbol.HasExplicitDefaultValue))
                        {
                            var hasDefaultValue = ArgumentHasDefaultValue(argumentMapping, c.SemanticModel);
                            if (argumentMapping.Node.NameColon == null && !hasDefaultValue)
                            {
                                return;
                            }
                            else if (hasDefaultValue)
                            {
                                c.ReportIssue(CreateDiagnostic(Rule, argumentMapping.Node.GetLocation(), argumentMapping.Symbol.Name));
                            }
                        }
                    }
                },
                SyntaxKind.InvocationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);

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
