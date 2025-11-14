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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AbstractClassToInterface : SonarDiagnosticAnalyzer<SyntaxKind>
{
    private const string DiagnosticId = "S1694";

    protected override string MessageFormat => "Convert this 'abstract' {0} to an interface.";
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    public AbstractClassToInterface() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterSymbolAction(
            c =>
            {
                var symbol = (INamedTypeSymbol)c.Symbol;
                if (symbol.IsClass()
                    && symbol.IsAbstract
                    && symbol.BaseType.Is(KnownType.System_Object)
                    && !IsRecordWithParameters(symbol)
                    && AllMethodsAreAbstract(symbol)
                    && !symbol.GetMembers().OfType<IFieldSymbol>().Any())
                {
                    foreach (var declaringSyntaxReference in symbol.DeclaringSyntaxReferences)
                    {
                        var node = declaringSyntaxReference.GetSyntax();
                        if (node is ClassDeclarationSyntax classDeclaration)
                        {
                            c.ReportIssue(Rule, classDeclaration.Identifier, "class");
                        }

                        if (RecordDeclarationSyntaxWrapper.IsInstance(node))
                        {
                            var wrapper = (RecordDeclarationSyntaxWrapper)node;
                            c.ReportIssue(Rule, wrapper.Identifier, "record");
                        }
                    }
                }
            },
            SymbolKind.NamedType);

    private static bool IsRecordWithParameters(ISymbol symbol) =>
        symbol.DeclaringSyntaxReferences.Any(x => x.GetSyntax() is { } node
                                                  && RecordDeclarationSyntaxWrapper.IsInstance(node)
                                                  && ((RecordDeclarationSyntaxWrapper)node).ParameterList is { Parameters.Count: > 0 });

    private static bool AllMethodsAreAbstract(INamedTypeSymbol symbol)
    {
        var methods = symbol.GetMembers().Where(x => x is IMethodSymbol { IsImplicitlyDeclared: false }).ToArray();
        return methods.Any() && Array.TrueForAll(methods, x => x.IsAbstract);
    }
}
