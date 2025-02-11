/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
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
                    if (c.Node.ArgumentList() is { Arguments.Count: > 0 } argumentList
                        && !c.IsRedundantPrimaryConstructorBaseTypeContext()
                        && !c.IsInExpressionTree() // Can't use optional arguments in expression trees (CS0584), so skip those
                        && new CSharpMethodParameterLookup(argumentList, c.Model) is { MethodSymbol: { } } methodParameterLookup)
                    {
                        foreach (var argumentMapping in methodParameterLookup.GetAllArgumentParameterMappings().Reverse().Where(x => ArgumentHasDefaultValue(x, c.Model)))
                        {
                            c.ReportIssue(Rule, argumentMapping.Node, argumentMapping.Symbol.Name);
                        }
                    }
                },
                SyntaxKind.InvocationExpression,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKindEx.ImplicitObjectCreationExpression,
                SyntaxKind.BaseConstructorInitializer,
                SyntaxKind.ThisConstructorInitializer,
                SyntaxKindEx.PrimaryConstructorBaseType);

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
