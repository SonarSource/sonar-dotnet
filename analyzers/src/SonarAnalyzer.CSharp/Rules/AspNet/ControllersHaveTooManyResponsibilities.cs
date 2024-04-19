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
using static SonarAnalyzer.Helpers.DisjointSetsPrimitives;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ControllersHaveTooManyResponsibilities : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6960";
    private const string MessageFormat = "This controller has multiple responsibilities and could be split into {0} smaller units.";

    private static readonly HashSet<string> ExcludedWellKnownServices =
    [
        "ILogger",
        "IMediator",
        "IMapper",
        "IConfiguration",
        "IBus",
        "IMessageBus"
    ];

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            if (compilationStartContext.Compilation.ReferencesControllers())
            {
                compilationStartContext.RegisterSymbolStartAction(symbolStartContext =>
                {
                    var symbol = (INamedTypeSymbol)symbolStartContext.Symbol;
                    if (symbol.IsControllerType()
                        && symbol.GetAttributesWithInherited().Any(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute)))
                    {
                        CheckApiController(symbolStartContext, symbol);
                    }
                }, SymbolKind.NamedType);
            }
        });

    private static void CheckApiController(SonarSymbolStartAnalysisContext symbolStartContext, INamedTypeSymbol controllerSymbol)
    {
        var memberNames = RelevantMemberNames(controllerSymbol);

        if (memberNames.Count < 2)
        {
            return;
        }

        var dependencies = new ConcurrentStack<Dependency>();

        symbolStartContext.RegisterCodeBlockStartAction<SyntaxKind>(codeBlockStartContext =>
        {
            if (BlockName(codeBlockStartContext.CodeBlock) is { } blockName)
            {
                codeBlockStartContext.RegisterNodeAction(c =>
                {
                    if (c.Node.GetName() is { } dependencyName && memberNames.Contains(dependencyName))
                    {
                        dependencies.Push(new(blockName, dependencyName));
                    }
                }, SyntaxKind.IdentifierName);
            }
        });

        symbolStartContext.RegisterSymbolEndAction(symbolEndContext =>
        {
            var parents = memberNames.ToDictionary(x => x, x => x); // Start with singleton sets
            foreach (var dependency in dependencies)
            {
                Union(parents, dependency.From, dependency.To);
            }

            var responsibilityGroups = DisjointSets(parents);
            if (responsibilityGroups.Count > 1)
            {
                var secondaryLocations = SecondaryLocations(controllerSymbol, responsibilityGroups);
                foreach (var primaryLocation in LocationIdentifiers<ClassDeclarationSyntax>(controllerSymbol))
                {
                    symbolEndContext.ReportIssue(Diagnostic.Create(
                        Rule,
                        primaryLocation,
                        secondaryLocations.ToAdditionalLocations(),
                        secondaryLocations.ToProperties(),
                        responsibilityGroups.Count));
                }
            }
        });
    }

    private static string BlockName(SyntaxNode block) =>
        block switch
        {
            MethodDeclarationSyntax method => method.GetName(),
            AccessorDeclarationSyntax { Parent.Parent: PropertyDeclarationSyntax property } => property.GetName(),
            _ => null
        };

    private static IEnumerable<Location> LocationIdentifiers<T>(ISymbol symbol) where T : SyntaxNode =>
        symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).OfType<T>().Select(x => x.GetIdentifier()?.GetLocation());

    private static ImmutableHashSet<string> RelevantMemberNames(INamedTypeSymbol symbol)
    {
        var builder = ImmutableHashSet.CreateBuilder<string>();
        foreach (var member in symbol.GetMembers())
        {
            switch (member)
            {
                // Constructors are not considered because they have to be split anyway
                // Accessors are not considered because they are part of properties, that are considered as a whole
                case IMethodSymbol method when !method.IsConstructor() && !method.IsStaticConstructor() && method.AssociatedSymbol is not IPropertySymbol:
                    builder.Add(method.Name);
                    break;
                // Primary constructor parameters may or may not generate fields, and must be considered
                case IMethodSymbol method when method.IsPrimaryConstructor():
                    builder.UnionWith(method.Parameters.Where(IsService).Select(x => x.Name));
                    break;
                // Backing fields are excluded for auto-properties, since they are considered part of the property
                case IFieldSymbol field when IsService(field) && !field.IsImplicitlyDeclared:
                    builder.Add(field.Name);
                    break;
                case IPropertySymbol property when IsService(property):
                    builder.Add(property.Name);
                    break;
            }
        }

        return builder.ToImmutable();
    }

    private static bool IsService(ISymbol symbol) =>
        symbol.GetSymbolType() is { TypeKind: TypeKind.Interface, Name: var name } && !ExcludedWellKnownServices.Contains(name);

    private static IEnumerable<SecondaryLocation> SecondaryLocations(INamedTypeSymbol controllerSymbol, List<List<string>> sets)
    {
        var methods = controllerSymbol.GetMembers().OfType<IMethodSymbol>().ToList();
        for (var setIndex = 0; setIndex < sets.Count; setIndex++)
        {
            foreach (var memberLocation in sets[setIndex].SelectMany(MemberLocations))
            {
                yield return new SecondaryLocation(memberLocation, $"Belongs to responsibility #{setIndex + 1}.");
            }
        }

        IEnumerable<Location> MemberLocations(string memberName) =>
            methods.Where(x => x.Name == memberName).SelectMany(LocationIdentifiers<MethodDeclarationSyntax>);
    }

    private record struct Dependency(string From, string To);
}
