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
    public sealed class InvalidCastToInterface : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1944";
        private const string MessageFormat = "{0}"; // This format string can be removed after we drop the old SE engine.
        private const string MessageReviewFormat = "Review this cast; in this project there's no type that {0}.";

        public static readonly DiagnosticDescriptor S1944 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(S1944);
        protected override bool EnableConcurrentExecution => false;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                compilationStartContext =>
                {
                    var allNamedTypeSymbols = compilationStartContext.Compilation.GlobalNamespace.GetAllNamedTypes();
                    var typeInterfaceMappings = allNamedTypeSymbols.Select(type =>
                        new
                        {
                            Type = type.OriginalDefinition,
                            Interfaces = type.OriginalDefinition.AllInterfaces.Select(i => i.OriginalDefinition)
                        });

                    var interfaceImplementerMappings = new Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>>();
                    foreach (var typeInterfaceMapping in typeInterfaceMappings)
                    {
                        if (typeInterfaceMapping.Type.IsInterface())
                        {
                            if (!interfaceImplementerMappings.ContainsKey(typeInterfaceMapping.Type))
                            {
                                interfaceImplementerMappings.Add(typeInterfaceMapping.Type, new HashSet<INamedTypeSymbol>());
                            }

                            interfaceImplementerMappings[typeInterfaceMapping.Type].Add(typeInterfaceMapping.Type);
                        }

                        foreach (var @interface in typeInterfaceMapping.Interfaces)
                        {
                            if (!interfaceImplementerMappings.ContainsKey(@interface))
                            {
                                interfaceImplementerMappings.Add(@interface, new HashSet<INamedTypeSymbol>());
                            }

                            interfaceImplementerMappings[@interface].Add(typeInterfaceMapping.Type);
                        }
                    }

                    compilationStartContext.RegisterNodeAction(
                        c =>
                        {
                            var cast = (CastExpressionSyntax)c.Node;
                            var interfaceType = c.SemanticModel.GetTypeInfo(cast.Type).Type as INamedTypeSymbol;
                            var expressionType = c.SemanticModel.GetTypeInfo(cast.Expression).Type as INamedTypeSymbol;

                            CheckTypesForInvalidCast(c, interfaceType, expressionType, interfaceImplementerMappings, cast.Type.GetLocation());
                        },
                        SyntaxKind.CastExpression);
                });
        }

        private static void CheckTypesForInvalidCast(SonarSyntaxNodeReportingContext context, INamedTypeSymbol interfaceType, INamedTypeSymbol expressionType,
            Dictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> interfaceImplementerMappings, Location issueLocation)
        {
            if (interfaceType == null ||
                expressionType == null ||
                !interfaceType.IsInterface() ||
                expressionType.Is(KnownType.System_Object))
            {
                return;
            }

            if (!HasExistingConcreteImplementation(interfaceType, interfaceImplementerMappings))
            {
                return;
            }

            if (expressionType.IsInterface() &&
                !HasExistingConcreteImplementation(expressionType, interfaceImplementerMappings))
            {
                return;
            }

            if (interfaceImplementerMappings.ContainsKey(interfaceType.OriginalDefinition) &&
                !interfaceImplementerMappings[interfaceType.OriginalDefinition].Any(t => t.DerivesOrImplements(expressionType.OriginalDefinition)) &&
                !expressionType.IsSealed)
            {
                ReportIssue(context, interfaceType, expressionType, issueLocation);
            }
        }

        private static bool HasExistingConcreteImplementation(INamedTypeSymbol type,
            IReadOnlyDictionary<INamedTypeSymbol, HashSet<INamedTypeSymbol>> interfaceImplementerMappings) =>
            interfaceImplementerMappings.ContainsKey(type) &&
            interfaceImplementerMappings[type].Any(t => t.IsClassOrStruct());

        private static void ReportIssue(SonarSyntaxNodeReportingContext context, ISymbol interfaceType, ITypeSymbol expressionType, Location issueLocation)
        {
            var interfaceTypeName = interfaceType.ToMinimalDisplayString(context.SemanticModel, issueLocation.SourceSpan.Start);
            var expressionTypeName = expressionType.ToMinimalDisplayString(context.SemanticModel, issueLocation.SourceSpan.Start);

            var messageReasoning = expressionType.IsInterface()
                ? $"implements both '{expressionTypeName}' and '{interfaceTypeName}'"
                : $"extends '{expressionTypeName}' and implements '{interfaceTypeName}'";

            context.ReportIssue(Diagnostic.Create(S1944, issueLocation, string.Format(MessageReviewFormat, messageReasoning)));
        }
    }
}
