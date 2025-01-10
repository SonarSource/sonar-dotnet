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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Core.Trackers;

public abstract class MethodDeclarationTracker<TSyntaxKind> : TrackerBase<TSyntaxKind, MethodDeclarationContext>
    where TSyntaxKind : struct
{
    public abstract Condition ParameterAtIndexIsUsed(int index);
    protected abstract SyntaxToken? GetMethodIdentifier(SyntaxNode methodDeclaration);

    public void Track(TrackerInput input, params Condition[] conditions)
    {
        input.Context.RegisterCompilationStartAction(c =>
            {
                if (input.IsEnabled(c.Options))
                {
                    c.RegisterSymbolAction(TrackMethodDeclaration, SymbolKind.Method);
                }
            });

        void TrackMethodDeclaration(SonarSymbolReportingContext c)
        {
            if (!IsTrackedMethod((IMethodSymbol)c.Symbol, c.Compilation))
            {
                return;
            }

            foreach (var declaration in c.Symbol.DeclaringSyntaxReferences)
            {
                if (c.Symbol.IsTopLevelMain())
                {
                    c.ReportIssue(Language.GeneratedCodeRecognizer, input.Rule, Location.Create(declaration.SyntaxTree, TextSpan.FromBounds(0, 0)));
                }
                else
                {
                    var methodIdentifier = GetMethodIdentifier(declaration.GetSyntax());
                    if (methodIdentifier.HasValue)
                    {
                        c.ReportIssue(Language.GeneratedCodeRecognizer, input.Rule, methodIdentifier.Value);
                    }
                }
            }
        }

        bool IsTrackedMethod(IMethodSymbol methodSymbol, Compilation compilation)
        {
            var conditionContext = new MethodDeclarationContext(methodSymbol, compilation);
            return conditions.All(c => c(conditionContext));
        }
    }

    public Condition MatchMethodName(params string[] methodNames) =>
        context => methodNames.Contains(context.MethodSymbol.Name);

    public Condition IsOrdinaryMethod() =>
        context => context.MethodSymbol.MethodKind == MethodKind.Ordinary;

    public Condition IsMainMethod() =>
        context => context.MethodSymbol.IsMainMethod()
                   || context.MethodSymbol.IsTopLevelMain();

    internal Condition AnyParameterIsOfType(params KnownType[] types)
    {
        var typesArray = types.ToImmutableArray();
        return context =>
            context.MethodSymbol.Parameters.Length > 0
            && context.MethodSymbol.Parameters.Any(parameter => parameter.Type.DerivesOrImplementsAny(typesArray));
    }

    internal Condition DecoratedWithAnyAttribute(params KnownType[] attributeTypes) =>
        context => context.MethodSymbol.GetAttributes().Any(a => a.AttributeClass.IsAny(attributeTypes));
}
