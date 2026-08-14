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

namespace SonarAnalyzer.Core.Rules
{
    public abstract class TooManyParametersBase<TSyntaxKind, TParameterListSyntax> : ParametrizedDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TParameterListSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S107";
        protected const string MessageFormat = "{0} has {1} parameters, which is greater than the {2} authorized.";
        private const int DefaultValueMaximum = 7;

        // These attributes identify parameters supplied by a framework, DI container, or runtime binding rather than by the caller.
        protected static readonly ImmutableArray<KnownType> DependencyInjectionAttributes = ImmutableArray.Create(
            KnownType.HotChocolate_EventMessageAttribute,
            KnownType.HotChocolate_GlobalStateAttribute,
            KnownType.HotChocolate_LocalStateAttribute,
            KnownType.HotChocolate_ParentAttribute,
            KnownType.HotChocolate_SchemaServiceAttribute,
            KnownType.HotChocolate_ScopedServiceAttribute,  // Obsolete since Hot Chocolate 13 and removed in 14
            KnownType.HotChocolate_ScopedStateAttribute,
            KnownType.HotChocolate_ServiceAttribute,
            KnownType.Microsoft_AspNetCore_Mvc_FromServicesAttribute,
            KnownType.Microsoft_AspNetCore_Mvc_FromStateAttribute,
            KnownType.Microsoft_Azure_WebJobs_BlobAttribute,
            KnownType.Microsoft_Azure_WebJobs_CosmosDBAttribute,
            KnownType.Microsoft_Extensions_DependencyInjection_FromKeyedServicesAttribute,
            KnownType.Orleans_Runtime_PersistentStateAttribute);

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract string UserFriendlyNameForNode(SyntaxNode node);
        protected abstract bool TryGetParameterCountAboveMaximum(TParameterListSyntax parameterList, SemanticModel model, out int parameterCount);
        protected abstract int BaseParameterCount(SyntaxNode node, SemanticModel model);
        protected abstract bool CanBeChanged(SyntaxNode node, SemanticModel model);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        [RuleParameter("max", PropertyType.Integer, "Maximum authorized number of parameters", DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        protected virtual bool IsExtern(SyntaxNode node) => false;

        protected TooManyParametersBase() =>
            rule = Language.CreateDescriptor(DiagnosticId, MessageFormat, isEnabledByDefault: false);

        protected override void Initialize(SonarParametrizedAnalysisContext context) =>
            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (TryGetParameterCountAboveMaximum((TParameterListSyntax)c.Node, c.Model, out var parametersCount)
                        && c.Node.Parent is { } parent
                        && !IsExtern(parent))
                    {
                        var baseCount = BaseParameterCount(parent, c.Model);
                        if (parametersCount - baseCount > Maximum && CanBeChanged(parent, c.Model))
                        {
                            var valueText = baseCount == 0 ? parametersCount.ToString() : $"{parametersCount - baseCount} new";
                            c.ReportIssue(SupportedDiagnostics[0], c.Node, UserFriendlyNameForNode(parent), valueText, Maximum.ToString());
                        }
                    }
                },
                Language.SyntaxKind.ParameterList);

        protected static bool VerifyCanBeChangedBySymbol(SyntaxNode node, SemanticModel semanticModel)
        {
            var declaredSymbol = semanticModel.GetDeclaredSymbol(node);
            var symbol = semanticModel.GetSymbolInfo(node).Symbol;
            if (declaredSymbol == null && symbol == null)
            {
                return false;
            }

            if (symbol != null)
            {
                return true;    // Not a declaration, such as Action
            }

            if (declaredSymbol.IsStatic)
            {
                if ((declaredSymbol.IsExtern && declaredSymbol.HasAttribute(KnownType.System_Runtime_InteropServices_DllImportAttribute))
                    || declaredSymbol.HasAttribute(KnownType.System_Runtime_InteropServices_LibraryImportAttribute))
                {
                    return false;   // P/Invoke method is defined externally.
                }
            }

            return declaredSymbol.OverriddenMember is null && declaredSymbol.InterfaceMembers().IsEmpty;
        }

        protected static bool IsDependencyInjected(IParameterSymbol parameter) =>
            parameter.AnyAttributeDerivesFromAny(DependencyInjectionAttributes);
    }
}
