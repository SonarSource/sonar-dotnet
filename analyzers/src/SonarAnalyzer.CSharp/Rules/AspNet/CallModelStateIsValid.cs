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

using System.Collections.Concurrent;
using SonarAnalyzer.Common.Walkers;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CallModelStateIsValid : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6967";
    private const string MessageFormat = "ModelState.IsValid should be checked in controller actions.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
        {
            if (compilationStart.Compilation.ReferencesControllers()
                && !compilationStart.Compilation.Assembly.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute))
            {
                compilationStart.RegisterSymbolStartAction(symbolStart =>
                {
                    var type = (INamedTypeSymbol)symbolStart.Symbol;
                    if (type.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase)
                        && !type.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute)
                        && !HasActionFilterAttribute(type))
                    {
                        symbolStart.RegisterSyntaxNodeAction(ProcessControllerMethods, SyntaxKind.MethodDeclaration);
                    }
                }, SymbolKind.NamedType);
            }
        });

    private static void ProcessControllerMethods(SonarSyntaxNodeReportingContext context)
    {
        var method = (MethodDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(context.Node) is IMethodSymbol methodSymbol
            && methodSymbol.IsControllerMethod()
            && !HasActionFilterAttribute(methodSymbol)
            && methodSymbol.Parameters.Any(x => HasValidationAttribute(x) || IsValidatable(x.Type)))
        {
            var walker = new ModelStateFinder(context.SemanticModel);
            walker.Visit(method.Body);
            if (!walker.ValidatesModel)
            {
                context.ReportIssue(Rule, method.Identifier);
            }
        }
    }

    private static bool IsValidatable(ITypeSymbol type) => type != null
        && (type.Implements(KnownType.System_ComponentModel_DataAnnotations_IValidatableObject)
            || type.GetMembers().OfType<IPropertySymbol>().Any(HasValidationAttribute)
            || IsValidatable(type.BaseType));

    private static bool HasValidationAttribute(ISymbol symbol) =>
        symbol.GetAttributes().Any(x => x.AttributeClass.DerivesFrom(KnownType.System_ComponentModel_DataAnnotations_ValidationAttribute));

    private static bool HasActionFilterAttribute(ISymbol symbol) =>
        symbol.GetAttributes().Any(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_Filters_ActionFilterAttribute));

    private sealed class ModelStateFinder : SafeCSharpSyntaxWalker
    {
        private readonly SemanticModel semanticModel;
        public bool ValidatesModel { get; private set; }

        public ModelStateFinder(SemanticModel semanticModel) => this.semanticModel = semanticModel;

        public override void Visit(SyntaxNode node)
        {
            if (!ValidatesModel)
            {
                base.Visit(node);
            }
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (node.GetName() == "TryValidateModel"
               && semanticModel.GetSymbolInfo(node.Expression).Symbol is IMethodSymbol method
               && method.ContainingType.Is(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase))
            {
                ValidatesModel = true;
            }
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (node.GetName() == "IsValid"
                && semanticModel.GetSymbolInfo(node.Expression).Symbol is IPropertySymbol { Name: "ModelState" } callerType
                && callerType.ContainingType.Is(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase))
            {
                ValidatesModel = true;
            }
        }
    }
}
