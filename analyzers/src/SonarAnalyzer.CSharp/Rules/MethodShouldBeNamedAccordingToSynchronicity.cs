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
    public sealed class MethodShouldBeNamedAccordingToSynchronicity : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4261";
        private const string MessageFormat = "{0}";
        private const string AddAsyncSuffixMessage = "Add the 'Async' suffix to the name of this method.";
        private const string RemoveAsyncSuffixMessage = "Remove the 'Async' suffix to the name of this method.";

        private static readonly ImmutableArray<KnownType> AsyncReturnTypes =
            ImmutableArray.Create(
                KnownType.System_Threading_Tasks_Task,
                KnownType.System_Threading_Tasks_Task_T,
                KnownType.System_Threading_Tasks_ValueTask, // NetCore 2.2+
                KnownType.System_Threading_Tasks_ValueTask_TResult);

        private static readonly ImmutableArray<KnownType> AsyncReturnInterfaces =
            ImmutableArray.Create(KnownType.System_Collections_Generic_IAsyncEnumerable_T);

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                    if (methodDeclaration.Identifier.IsMissing)
                    {
                        return;
                    }

                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodDeclaration);
                    if (methodSymbol == null
                        || methodSymbol.IsMainMethod()
                        || methodSymbol.GetInterfaceMember() != null
                        || methodSymbol.GetOverriddenMember() != null
                        || methodSymbol.IsTestMethod()
                        || methodSymbol.IsControllerActionMethod()
                        || IsSignalRHubMethod(methodSymbol))
                    {
                        return;
                    }

                    var hasAsyncReturnType = HasAsyncReturnType(methodSymbol);
                    var hasAsyncSuffix = HasAsyncSuffix(methodDeclaration);

                    if (hasAsyncSuffix && !hasAsyncReturnType)
                    {
                        c.ReportIssue(Rule, methodDeclaration.Identifier, RemoveAsyncSuffixMessage);
                    }
                    else if (!hasAsyncSuffix && hasAsyncReturnType)
                    {
                        c.ReportIssue(Rule, methodDeclaration.Identifier, AddAsyncSuffixMessage);
                    }
                },
                SyntaxKind.MethodDeclaration);

        private static bool HasAsyncReturnType(IMethodSymbol methodSymbol) =>
            methodSymbol.ReturnType is ITypeParameterSymbol typeParameter
                ? typeParameter.ConstraintTypes.Any(IsAsyncType)
                : (methodSymbol.ReturnType as INamedTypeSymbol)?.ConstructedFrom is { } returnSymbol
                  && !returnSymbol.Is(KnownType.Void)
                  && IsAsyncType(returnSymbol);

        private static bool IsAsyncType(ITypeSymbol typeSymbol) =>
            typeSymbol.DerivesFromAny(AsyncReturnTypes)
            || typeSymbol.IsAny(AsyncReturnInterfaces)
            || typeSymbol.ImplementsAny(AsyncReturnInterfaces);

        private static bool HasAsyncSuffix(MethodDeclarationSyntax methodDeclaration) =>
            methodDeclaration.Identifier.ValueText.EndsWith("async", StringComparison.OrdinalIgnoreCase);

        private static bool IsSignalRHubMethod(ISymbol methodSymbol) =>
            methodSymbol.GetEffectiveAccessibility() == Accessibility.Public
            && IsSignalRHubMethod(methodSymbol.ContainingType);

        private static bool IsSignalRHubMethod(ITypeSymbol typeSymbol) =>
            typeSymbol.DerivesFrom(KnownType.Microsoft_AspNet_SignalR_Hub);
    }
}
