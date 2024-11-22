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

using System.Collections.Concurrent;

namespace SonarAnalyzer.Rules;

public abstract class RouteTemplateShouldNotStartWithSlashBase<TSyntaxKind>() : SonarDiagnosticAnalyzer<TSyntaxKind>(DiagnosticId)
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6931";
    private const string MessageOnlyActions = "Change the paths of the actions of this controller to be relative and adapt the controller route accordingly.";
    private const string MessageActionsAndController = "Change the paths of the actions of this controller to be relative and add a controller route with the common prefix.";

    protected override string MessageFormat => "{0}";

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            if (!compilationStartContext.Compilation.ReferencesNetCoreControllers()
                && !compilationStartContext.Compilation.ReferencesNetFrameworkControllers())
            {
                return;
            }

            compilationStartContext.RegisterSymbolStartAction(symbolStartContext =>
            {
                var symbol = (INamedTypeSymbol)symbolStartContext.Symbol;
                if (symbol.IsControllerType())
                {
                    var controllerActionInfo = new ConcurrentStack<ActionParametersInfo>();
                    symbolStartContext.RegisterSyntaxNodeAction(nodeContext =>
                    {
                        if (nodeContext.SemanticModel.GetDeclaredSymbol(nodeContext.Node) is IMethodSymbol methodSymbol && methodSymbol.IsControllerActionMethod())
                        {
                            controllerActionInfo.Push(new ActionParametersInfo(RouteAttributeTemplateArguments(methodSymbol.GetAttributes())));
                        }
                    }, Language.SyntaxKind.MethodDeclarations);

                    symbolStartContext.RegisterSymbolEndAction(symbolEndContext =>
                        ReportIssues(symbolEndContext, symbol, controllerActionInfo));
                }
            }, SymbolKind.NamedType);
        });

    private void ReportIssues(SonarSymbolReportingContext context, INamedTypeSymbol controllerSymbol, ConcurrentStack<ActionParametersInfo> actions)
    {
        // If one of the following conditions is true, the rule won't raise an issue
        // 1. The controller does not have any actions defined
        // 2. At least one action is not annotated with a route attribute or is annotated with a parameterless attribute
        // 3. There is at least one action with a route template that does not start with '/'
        if (!actions.Any() || actions.Any(x => !x.RouteParameters.Any() || x.RouteParameters.Values.Any(x => !x.StartsWith("/") && !x.StartsWith("~/"))))
        {
            return;
        }

        var issueMessage = controllerSymbol.GetAttributes().Any(x => x.AttributeClass.IsAny(KnownType.RouteAttributes) || x.AttributeClass.Is(KnownType.System_Web_Mvc_RoutePrefixAttribute))
            ? MessageOnlyActions
            : MessageActionsAndController;

        var secondaryLocations = actions.SelectMany(x => x.RouteParameters.Keys.ToSecondary());
        foreach (var identifier in controllerSymbol.DeclaringSyntaxReferences.Select(x => Language.Syntax.NodeIdentifier(x.GetSyntax())).WhereNotNull())
        {
            context.ReportIssue(Language.GeneratedCodeRecognizer, Rule, identifier.GetLocation(), secondaryLocations, issueMessage);
        }
    }

    private static Dictionary<Location, string> RouteAttributeTemplateArguments(ImmutableArray<AttributeData> attributes)
    {
        var templates = new Dictionary<Location, string>();
        foreach (var attribute in attributes)
        {
            if (attribute.GetAttributeRouteTemplate() is { } templateParameter)
            {
                templates.Add(attribute.ApplicationSyntaxReference.GetSyntax().GetLocation(), templateParameter);
            }
        }
        return templates;
    }

    private readonly record struct ActionParametersInfo(Dictionary<Location, string> RouteParameters);
}
