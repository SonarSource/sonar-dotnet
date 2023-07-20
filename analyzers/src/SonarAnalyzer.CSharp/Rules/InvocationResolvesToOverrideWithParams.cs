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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InvocationResolvesToOverrideWithParams : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3220";
    private const string MessageFormat = "Review this call, which partially matches an overload without 'params'. The partial match is '{0}'.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            c =>
            {
                var invocation = (InvocationExpressionSyntax)c.Node;
                CheckCall(c, invocation, invocation.ArgumentList);
            },
            SyntaxKind.InvocationExpression);

        context.RegisterNodeAction(
            c =>
            {
                var objectCreation = (ObjectCreationExpressionSyntax)c.Node;
                CheckCall(c, objectCreation, objectCreation.ArgumentList);
            },
            SyntaxKind.ObjectCreationExpression);
    }

    private static void CheckCall(SonarSyntaxNodeReportingContext context, SyntaxNode node, ArgumentListSyntax argumentList)
    {
        if (argumentList is { Arguments.Count: > 0 }
            && context.SemanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol method
            && method.Parameters.LastOrDefault() is { IsParams: true }
            && !IsInvocationWithExplicitArray(argumentList, method, context.SemanticModel)
            && ArgumentTypes(context, argumentList) is var argumentTypes
            && argumentTypes.All(x => x is not IErrorTypeSymbol)
            && OtherOverloadsOf(method).FirstOrDefault(IsPossibleMatch) is { } otherMethod)
        {
            context.ReportIssue(CreateDiagnostic(Rule, node.GetLocation(), otherMethod.ToMinimalDisplayString(context.SemanticModel, node.SpanStart)));
        }

        bool IsPossibleMatch(IMethodSymbol method) =>
            ArgumentsMatchParameters(argumentList, argumentTypes, method, context.SemanticModel) && MethodAccessibleWithinType(method, context.ContainingSymbol.ContainingType);
    }

    private static ITypeSymbol[] ArgumentTypes(SonarSyntaxNodeReportingContext context, ArgumentListSyntax argumentList) =>
        argumentList.Arguments
            .Select(x => context.SemanticModel.GetTypeInfo(x.Expression))
            .Select(x => x.Type ?? x.ConvertedType) // Action and Func won't always resolve properly with Type
            .ToArray();

    private static IEnumerable<IMethodSymbol> OtherOverloadsOf(IMethodSymbol method) =>
        method.ContainingType
            .GetMembers(method.Name)
            .OfType<IMethodSymbol>()
            .Where(x => !x.IsVararg && x.MethodKind == method.MethodKind && !x.Equals(method) && x.Parameters.Any() && !x.Parameters.Last().IsParams);

    private static bool IsInvocationWithExplicitArray(ArgumentListSyntax argumentList, IMethodSymbol invokedMethodSymbol, SemanticModel semanticModel)
    {
        var lookup = new CSharpMethodParameterLookup(argumentList, invokedMethodSymbol);
        var parameters = argumentList.Arguments.Select(Valid).ToArray();
        return parameters.All(x => x is not null) && parameters.Count(x => x.IsParams) == 1;

        IParameterSymbol Valid(ArgumentSyntax argument) =>
            lookup.TryGetSymbol(argument, out var parameter)
            && (!parameter.IsParams || semanticModel.GetTypeInfo(argument.Expression).Type is IArrayTypeSymbol)
                ? parameter
                : null;
    }

    private static bool ArgumentsMatchParameters(ArgumentListSyntax argumentList, ITypeSymbol[] argumentTypes, IMethodSymbol possibleOtherMethod, SemanticModel semanticModel)
    {
        var lookup = new CSharpMethodParameterLookup(argumentList, possibleOtherMethod);
        var parameters = argumentList.Arguments.Select((argument, index) => Valid(argument, argumentTypes[index])).ToArray();
        return parameters.All(x => x is not null) && possibleOtherMethod.Parameters.Except(parameters).All(x => x.HasExplicitDefaultValue);

        IParameterSymbol Valid(ArgumentSyntax argument, ITypeSymbol type) =>
            lookup.TryGetSymbol(argument, out var parameter)
            && ((type is INamedTypeSymbol && semanticModel.ClassifyConversion(argument.Expression, parameter.Type).IsImplicit)
                || (type is not INamedTypeSymbol && parameter.Type.IsReferenceType))
                ? parameter
                : null;
    }

    private static bool MethodAccessibleWithinType(IMethodSymbol method, ITypeSymbol type) =>
        IsInTypeOrNested(method, type)
        || method.DeclaredAccessibility switch
            {
                Accessibility.Private => false,
                // ProtectedAndInternal corresponds to `private protected`.
                Accessibility.ProtectedAndInternal => type.DerivesFrom(method.ContainingType) && method.IsInSameAssembly(type),
                // ProtectedOrInternal corresponds to `protected internal`.
                Accessibility.ProtectedOrInternal => type.DerivesFrom(method.ContainingType) || method.IsInSameAssembly(type),
                Accessibility.Protected => type.DerivesFrom(method.ContainingType),
                Accessibility.Internal => method.IsInSameAssembly(type),
                Accessibility.Public => true,
                _ => false,
            };

    private static bool IsInTypeOrNested(IMethodSymbol method, ITypeSymbol type) =>
        type is not null && (method.IsInType(type) || IsInTypeOrNested(method, type.ContainingType));
}
