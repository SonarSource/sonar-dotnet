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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CallModelStateIsValid : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6967";
    private const string MessageFormat = "ModelState.IsValid should be checked in controller actions.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly SyntaxKind[] PropertyAccessSyntaxNodesToVisit = [
        SyntaxKind.ConditionalAccessExpression,
        SyntaxKind.SimpleMemberAccessExpression,
        SyntaxKindEx.Subpattern];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
            {
                // The rule ignores the project completely if any of these conditions are met:
                // - the project doesn't reference ASP.NET MVC
                // - the project references the FluentValidation library:
                //      - as an alternative to using ModelState.IsValid
                //      - this can made more accurate: check if those validation methods are used in the controller actions rather than just checking whether the library is referenced
                // - the [ApiController] attribute is applied on the assembly level: this results in the attribute being applied to every Controller class in the project
                if (compilationStart.Compilation.ReferencesNetCoreControllers()
                    && compilationStart.Compilation.GetTypeByMetadataName(KnownType.FluentValidation_IValidator) is null
                    && !compilationStart.Compilation.Assembly.HasAttribute(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute))
                {
                    compilationStart.RegisterSymbolStartAction(symbolStart =>
                        {
                            var type = (INamedTypeSymbol)symbolStart.Symbol;
                            if (type.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase)
                                && !HasApiControllerAttribute(type)
                                && !HasActionFilterAttribute(type))
                            {
                                symbolStart.RegisterCodeBlockStartAction<SyntaxKind>(ProcessCodeBlock);
                            }
                        },
                        SymbolKind.NamedType);
                }
            });

    private static void ProcessCodeBlock(SonarCodeBlockStartAnalysisContext<SyntaxKind> codeBlockContext)
    {
        if (codeBlockContext.CodeBlock is MethodDeclarationSyntax methodDeclaration
            && codeBlockContext.OwningSymbol is IMethodSymbol methodSymbol
            && methodSymbol.Parameters.Any(RequiresValidation)
            && methodSymbol.IsControllerActionMethod()
            && !HasActionFilterAttribute(methodSymbol))
        {
            var isModelValidated = false;
            codeBlockContext.RegisterNodeAction(nodeContext =>
                {
                    if (!isModelValidated)
                    {
                        isModelValidated = IsCheckingValidityProperty(nodeContext.Node, nodeContext.Model);
                    }
                },
                PropertyAccessSyntaxNodesToVisit);

            codeBlockContext.RegisterNodeAction(nodeContext =>
                {
                    if (!isModelValidated)
                    {
                        isModelValidated = IsTryValidateInvocation(nodeContext.Node, nodeContext.Model);
                    }
                },
                SyntaxKind.InvocationExpression);

            codeBlockContext.RegisterCodeBlockEndAction(blockEnd =>
                {
                    if (!isModelValidated)
                    {
                        blockEnd.ReportIssue(Rule, methodDeclaration.Identifier);
                    }
                });
        }
    }

    // The rule raises for an action parameter only when validation actually applies to it. Mirroring the
    // recursive traversal ASP.NET performs (ValidationVisitor walks the object graph), the parameter needs
    // validation when the parameter itself, the parameter type (or one of its base types) or one of its
    // instance members is decorated with a validation attribute, when the type implements IValidatableObject,
    // when - for arrays and collections - an element type requires validation, or when a nested complex
    // member type requires validation. To avoid walking the framework object graph, recursion into member
    // types is limited to types declared in source; the visited set guards against cyclic graphs. Types with
    // no such surface (primitives, dynamic, framework types, collections of such types) yield nothing to
    // validate, matching the RSPEC: the rule does not raise when neither the model nor its members are
    // decorated with validation attributes.
    private static bool RequiresValidation(IParameterSymbol parameter) =>
        HasValidationAttribute(parameter)
        || RequiresValidation(parameter.Type, []);

    private static bool RequiresValidation(ITypeSymbol type, HashSet<ITypeSymbol> visited) =>
        type.TypeKind is not TypeKind.Dynamic
        && visited.Add(type)
        && (type.Implements(KnownType.System_ComponentModel_DataAnnotations_IValidatableObject)
            || type.GetSelfAndBaseTypes().Any(HasValidationSurface)
            || ElementTypesToValidate(type).Any(x => RequiresValidation(x, visited))
            || (type.DeclaringSyntaxReferences.Length > 0 && MemberTypes(type).Any(x => RequiresValidation(x, visited))));

    // ASP.NET model validation recurses into arrays and collections, validating each element, so the
    // rule inspects the element type(s) of arrays and IEnumerable<T> implementations. Both the collection's
    // own type arguments (e.g. the value type of Dictionary<TKey, TValue>) and the type argument of the
    // implemented IEnumerable<T> interface (e.g. for a non-generic subclass like CustomList : List<Model>)
    // are considered.
    private static IEnumerable<ITypeSymbol> ElementTypesToValidate(ITypeSymbol type) =>
        type switch
        {
            IArrayTypeSymbol array => [array.ElementType],
            INamedTypeSymbol collection when collection.DerivesOrImplements(KnownType.System_Collections_Generic_IEnumerable_T) =>
                collection.TypeArguments.Concat(collection.AllInterfaces
                    .Where(x => x.Is(KnownType.System_Collections_Generic_IEnumerable_T))
                    .SelectMany(x => x.TypeArguments)),
            _ => [],
        };

    // The types of the instance members (properties and fields) declared on the type or its base types.
    // ASP.NET validation recurses into these nested complex members.
    private static IEnumerable<ITypeSymbol> MemberTypes(ITypeSymbol type) =>
        type.GetSelfAndBaseTypes()
            .SelectMany(x => x.GetMembers())
            .Select(x => x switch
            {
                IPropertySymbol { IsStatic: false } property => property.Type,
                IFieldSymbol { IsStatic: false } field => field.Type,
                _ => null,
            })
            .Where(x => x is not null);

    // A type contributes validation surface when the type itself, or one of its declared instance
    // members (property or field), is decorated with a validation attribute.
    private static bool HasValidationSurface(INamedTypeSymbol type) =>
        HasValidationAttribute(type)
        || type.GetMembers().Any(x => x is IPropertySymbol { IsStatic: false } or IFieldSymbol { IsStatic: false } && HasValidationAttribute(x));

    private static bool HasValidationAttribute(ISymbol symbol) =>
        symbol.GetAttributes().Any(x => x.AttributeClass.DerivesFrom(KnownType.System_ComponentModel_DataAnnotations_ValidationAttribute));

    private static bool HasApiControllerAttribute(ITypeSymbol type) =>
        type.AttributesWithInherited.Any(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute));

    private static bool HasActionFilterAttribute(ISymbol symbol) =>
        symbol.AttributesWithInherited.Any(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_Filters_ActionFilterAttribute));

    private static bool IsCheckingValidityProperty(SyntaxNode node, SemanticModel model) =>
        node.GetIdentifier() is { ValueText: "IsValid" or "ValidationState" } nodeIdentifier
        && model.GetSymbolInfo(nodeIdentifier.Parent).Symbol is IPropertySymbol propertySymbol
        && propertySymbol.ContainingType.Is(KnownType.Microsoft_AspNetCore_Mvc_ModelBinding_ModelStateDictionary);

    private static bool IsTryValidateInvocation(SyntaxNode node, SemanticModel model) =>
        node is InvocationExpressionSyntax invocation
        && invocation.GetName() == "TryValidateModel"
        && model.GetSymbolInfo(invocation.Expression).Symbol is IMethodSymbol method
        && method.ContainingType.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase);
}
