/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SuppressFinalizeUseless : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3234";
        private const string MessageFormat = "Remove this useless call to 'GC.SuppressFinalize'.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    var suppressFinalizeSymbol = c.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;

                    if (suppressFinalizeSymbol?.Name != "SuppressFinalize" ||
                        !invocation.HasExactlyNArguments(1) ||
                        !suppressFinalizeSymbol.IsInType(KnownType.System_GC))
                    {
                        return;
                    }

                    var argument = invocation.ArgumentList.Arguments.First();
                    var argumentType = c.SemanticModel.GetTypeInfo(argument.Expression).Type as INamedTypeSymbol;

                    if (!argumentType.IsClass() ||
                        !argumentType.IsSealed)
                    {
                        return;
                    }

                    var hasFinalizer = argumentType.GetSelfAndBaseTypes()
                        .Where(type => !type.Is(KnownType.System_Object))
                        .SelectMany(type => type.GetMembers())
                        .OfType<IMethodSymbol>()
                        .Any(methodSymbol => methodSymbol.MethodKind == MethodKind.Destructor);

                    if (!hasFinalizer)
                    {
                        c.ReportIssue(rule, invocation);
                    }
                },
                SyntaxKind.InvocationExpression);
        }
    }
}
