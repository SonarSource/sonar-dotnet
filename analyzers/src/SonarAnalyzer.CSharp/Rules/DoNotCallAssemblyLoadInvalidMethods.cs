/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotCallAssemblyLoadInvalidMethods : DoNotCallMethodsCSharpBase
{
    private const string DiagnosticId = "S3885";

    private static readonly HashSet<SyntaxKind> EventHandlerSyntaxes =
    [
        SyntaxKind.MethodDeclaration,
        SyntaxKind.ParenthesizedLambdaExpression,
        SyntaxKind.AnonymousMethodExpression,
        SyntaxKindEx.LocalFunctionStatement
    ];

    protected override string MessageFormat => "Replace this call to '{0}' with 'Assembly.Load'.";

    protected override IEnumerable<MemberDescriptor> CheckedMethods { get; } = new List<MemberDescriptor>
    {
        new(KnownType.System_Reflection_Assembly, "LoadFrom"),
        new(KnownType.System_Reflection_Assembly, "LoadFile"),
        new(KnownType.System_Reflection_Assembly, "LoadWithPartialName")
    };

    public DoNotCallAssemblyLoadInvalidMethods() : base(DiagnosticId) { }

    protected override bool IsInValidContext(InvocationExpressionSyntax invocationSyntax, SemanticModel semanticModel) =>
        !IsInResolutionHandler(invocationSyntax, semanticModel);

    // Checks if the invocation is inside an event handler for the AppDomain.AssemblyResolve event.
    // This check creates FN for the other Resolution events:
    // AppDomain.TypeResolve, AppDomain.ResourceResolve, AppDomain.ReflectionOnlyAssemblyResolve.
    // https://learn.microsoft.com/en-us/dotnet/api/system.resolveeventargs
    private static bool IsInResolutionHandler(InvocationExpressionSyntax invocationSyntax, SemanticModel model) =>
        invocationSyntax
            .AncestorsAndSelf()
            .Any(x => x.IsAnyKind(EventHandlerSyntaxes)
                        && (model.GetSymbolInfo(x).Symbol ?? model.GetDeclaredSymbol(x)) is IMethodSymbol methodSymbol
                        && methodSymbol.IsEventHandler()
                        && methodSymbol.Parameters[1].Type.Is(KnownType.System_ResolveEventArgs));
}
