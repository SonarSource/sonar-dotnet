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

namespace SonarAnalyzer.Rules;

public abstract class BackslashShouldBeAvoidedInAspNetRoutesBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6930";

    protected abstract TSyntaxKind[] SyntaxKinds { get; }

    protected override string MessageFormat => @"Replace '\' with '/'.";

    protected BackslashShouldBeAvoidedInAspNetRoutesBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            if (compilationStartContext.Compilation.ReferencesControllers())
            {
                compilationStartContext.RegisterNodeAction(Language.GeneratedCodeRecognizer, Check, SyntaxKinds);
            }
        });

    protected void Check(SonarSyntaxNodeReportingContext c)
    {
        if (Language.Syntax.NodeExpression(c.Node) is { } expression
            && Language.FindConstantValue(c.SemanticModel, expression) is string constantRouteTemplate
            && ContainsBackslash(constantRouteTemplate)
            && IsRouteTemplate(c.SemanticModel, c.Node))
        {
            c.ReportIssue(Rule, expression);
        }
    }

    private bool IsRouteTemplate(SemanticModel model, SyntaxNode node) =>
        node.Parent.Parent is var invocation // can be a method invocation or a tuple expression
        && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
        && Language.MethodParameterLookup(invocation, methodSymbol) is { } parameterLookup
        && parameterLookup.TryGetSymbol(node, out var parameter)
        && (HasStringSyntaxAttributeOfTypeRoute(parameter) || IsRouteTemplateBeforeAspNet6(parameter, methodSymbol));

    private static bool HasStringSyntaxAttributeOfTypeRoute(IParameterSymbol parameter) =>
        parameter.GetAttributes(KnownType.System_Diagnostics_CodeAnalysis_StringSyntaxAttribute).FirstOrDefault() is { } syntaxAttribute
        && syntaxAttribute.TryGetAttributeValue<string>("syntax", out var syntaxParameter)
        && string.Equals(syntaxParameter, "Route", StringComparison.Ordinal);

    private static bool IsRouteTemplateBeforeAspNet6(IParameterSymbol parameter, IMethodSymbol method) =>
        // Remark: route templates cannot be specified via HttpXAttribute in ASP.NET 4.x
        (method.ContainingType.IsAny(KnownType.RouteAttributes)
            || method.ContainingType.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_Routing_HttpMethodAttribute))
        && method.IsConstructor() && parameter.Name == "template";

    private static bool ContainsBackslash(string value)
    {
        var firstBackslashIndex = value.IndexOf('\\');
        if (firstBackslashIndex < 0)
        {
            return false;
        }

        var firstRegexIndex = value.IndexOf("regex", StringComparison.Ordinal);
        return firstRegexIndex < 0 || firstBackslashIndex < firstRegexIndex;
    }
}
