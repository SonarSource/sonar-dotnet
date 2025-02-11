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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ControllersReuseClient : ReuseClientBase
{
    private const string DiagnosticId = "S6962";
    private const string MessageFormat = "Reuse HttpClient instances rather than create new ones with each controller action invocation.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override ImmutableArray<KnownType> ReusableClients => ImmutableArray.Create(KnownType.System_Net_Http_HttpClient);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            if (!compilationStartContext.Compilation.ReferencesNetCoreControllers())
            {
                return;
            }
            compilationStartContext.RegisterSymbolStartAction(symbolStartContext =>
            {
                var symbol = (INamedTypeSymbol)symbolStartContext.Symbol;
                if (symbol.IsControllerType())
                {
                    symbolStartContext.RegisterSyntaxNodeAction(nodeContext =>
                    {
                        var node = nodeContext.Node;
                        if (IsInPublicMethod(node)
                            && IsReusableClient(nodeContext)
                            && !IsInsideConstructor(node)
                            && !IsAssignedForReuse(nodeContext))
                        {
                            nodeContext.ReportIssue(Rule, node);
                        }
                    },
                    SyntaxKind.ObjectCreationExpression,
                    SyntaxKindEx.ImplicitObjectCreationExpression);
                }
            }, SymbolKind.NamedType);
        });

    public static bool IsInPublicMethod(SyntaxNode node) =>
        node.FirstAncestorOrSelf<MethodDeclarationSyntax>() is not { } method
        || (method.FirstAncestorOrSelf<PropertyDeclarationSyntax>() is null
            && method.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword)));

    private static bool IsInsideConstructor(SyntaxNode node) =>
        node.HasAncestor(SyntaxKind.ConstructorDeclaration, SyntaxKindEx.PrimaryConstructorBaseType);
}
