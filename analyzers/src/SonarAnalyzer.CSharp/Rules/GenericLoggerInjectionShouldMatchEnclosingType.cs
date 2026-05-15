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
public sealed class GenericLoggerInjectionShouldMatchEnclosingType : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6672";
    private const string MessageFormat = "Update this logger to use its enclosing type.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(cc =>
            {
                if (cc.Compilation.References(KnownAssembly.MicrosoftExtensionsLoggingAbstractions))
                {
                    cc.RegisterNodeAction(c =>
                        {
                            var invalidTypes = c.Node switch
                            {
                                ConstructorDeclarationSyntax x => InvalidTypeParameters(x, c.Model),
                                TypeDeclarationSyntax x => InvalidTypeParameters(x, c.Model),
                                _ => [],
                            };
                            foreach (var invalidType in invalidTypes)
                            {
                                c.ReportIssue(Rule, invalidType);
                            }
                        },
                        SyntaxKind.ConstructorDeclaration,
                        SyntaxKind.ClassDeclaration,
                        SyntaxKindEx.RecordDeclaration);
                }
            });

    // Returns T for [Constructor(ILogger<T> logger)] where T is not Constructor
    private static IEnumerable<TypeSyntax> InvalidTypeParameters(ConstructorDeclarationSyntax constructor, SemanticModel model) =>
        model.GetDeclaredSymbol(constructor)?.ContainingType is { IsValueType: false } constructorType
            ? CompareGenericParameters(constructor.ParameterList, constructorType, model)
            : [];

    // Only applies to primary constructors. ParameterList() returns null for non-primary type declarations.
    private static IEnumerable<TypeSyntax> InvalidTypeParameters(TypeDeclarationSyntax typeDeclaration, SemanticModel model) =>
        typeDeclaration.ParameterList() is { } parameterList
            ? CompareGenericParameters(parameterList, model.GetDeclaredSymbol(typeDeclaration), model)
            : [];

    private static IEnumerable<TypeSyntax> CompareGenericParameters(ParameterListSyntax parameterList, INamedTypeSymbol constructorType, SemanticModel model)
    {
        if (constructorType is null)
        {
            yield break;
        }

        var genericParameters = parameterList.Parameters
            .Where(x => x.Type is GenericNameSyntax generic
                        && generic.TypeArgumentList.Arguments.Count == 1)
            .Select(x => (GenericNameSyntax)x.Type)
            .ToArray();

        if (genericParameters.Length == 0)
        {
            yield break;
        }

        foreach (var generic in genericParameters)
        {
            var genericArgument = generic.TypeArgumentList.Arguments[0];
            if (IsGenericLogger(model.GetTypeInfo(generic).Type)                             // ILogger<T>
                && !model.GetTypeInfo(genericArgument).Type.Equals(constructorType))         // T
            {
                yield return genericArgument;
            }
        }
    }

    private static bool IsGenericLogger(ITypeSymbol type) =>
        type.Is(KnownType.Microsoft_Extensions_Logging_ILogger_TCategoryName)
        || type.Implements(KnownType.Microsoft_Extensions_Logging_ILogger_TCategoryName);
}
