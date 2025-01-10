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

namespace SonarAnalyzer.Rules.CSharp;

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
                    var constructor = (ConstructorDeclarationSyntax)c.Node;
                    foreach (var invalidType in InvalidTypeParameters(constructor, c.SemanticModel))
                    {
                        c.ReportIssue(Rule, invalidType);
                    }
                },
                SyntaxKind.ConstructorDeclaration);
            }
        });

    // Returns T for [Constructor(ILogger<T> logger)] where T is not Constructor
    private static IEnumerable<TypeSyntax> InvalidTypeParameters(ConstructorDeclarationSyntax constructor, SemanticModel model)
    {
        var genericParameters = constructor.ParameterList.Parameters
            .Where(x => x.Type is GenericNameSyntax generic
                        && generic.TypeArgumentList.Arguments.Count == 1)
            .Select(x => (GenericNameSyntax)x.Type)
            .ToArray();

        if (genericParameters.Length == 0)
        {
            yield break;
        }

        var constructorType = model.GetDeclaredSymbol(constructor)?.ContainingType;
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
