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

using System.Collections.Concurrent;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ControllersHaveMixedResponsibilities : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6960";
    private const string MessageFormat = "This controller has multiple responsibilities and could be split into {0} smaller controllers.";
    private const string UnspeakableIndexerName = "<indexer>I"; // All indexers are considered part of the same group

    public enum MemberType
    {
        Service,
        Action,
    }

    private static readonly HashSet<string> ExcludedWellKnownServices =
    [
        "ILogger",
        "IMediator",
        "IMapper",
        "IConfiguration",
        "IBus",
        "IMessageBus",
        "IHttpClientFactory"
    ];

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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
                if (symbol.IsCoreApiController()
                    && symbol.BaseType.Is(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase)
                    && !symbol.IsAbstract)
                {
                    var relevantMembers = RelevantMembers(symbol);

                    if (relevantMembers.Count < 2)
                    {
                        return;
                    }

                    var dependencies = new ConcurrentStack<Dependency>();
                    symbolStartContext.RegisterCodeBlockStartAction(PopulateDependencies(relevantMembers, dependencies));
                    symbolStartContext.RegisterSymbolEndAction(CalculateAndReportOnResponsibilities(symbol, relevantMembers, dependencies));
                }
                }, SymbolKind.NamedType);
        });

    private static Action<SonarCodeBlockStartAnalysisContext<SyntaxKind>> PopulateDependencies(
        ImmutableDictionary<string, MemberType> relevantMembers,
        ConcurrentStack<Dependency> dependencies) =>
        codeBlockStartContext =>
        {
            if (BlockName(codeBlockStartContext.CodeBlock) is { } blockName)
            {
                codeBlockStartContext.RegisterNodeAction(c =>
                {
                    if (c.Node.GetName() is { } dependencyName && relevantMembers.ContainsKey(blockName) && relevantMembers.ContainsKey(dependencyName))
                    {
                        dependencies.Push(new(blockName, dependencyName));
                    }
                }, SyntaxKind.IdentifierName);
            }
        };

    private static Action<SonarSymbolReportingContext> CalculateAndReportOnResponsibilities(
        INamedTypeSymbol controllerSymbol,
        ImmutableDictionary<string, MemberType> relevantMembers,
        ConcurrentStack<Dependency> dependencies) =>
        symbolEndContext =>
        {
            if (ResponsibilityGroups(relevantMembers, dependencies) is { Count: > 1 } responsibilityGroups)
            {
                var secondaryLocations = SecondaryLocations(controllerSymbol, responsibilityGroups).ToList();
                foreach (var primaryLocation in IdentifierLocations<ClassDeclarationSyntax>(controllerSymbol))
                {
                    symbolEndContext.ReportIssue(Rule, primaryLocation, secondaryLocations, responsibilityGroups.Count.ToString());
                }
            }
        };

    private static List<List<string>> ResponsibilityGroups(
        ImmutableDictionary<string, MemberType> relevantMembers,
        ConcurrentStack<Dependency> dependencies)
    {
        var dependencySets = new DisjointSets(relevantMembers.Keys);
        foreach (var dependency in dependencies)
        {
            dependencySets.Union(dependency.From, dependency.To);
        }

        return dependencySets
            .GetAllSets()
            // Filter out sets of only actions or only services
            .Where(x => x.Exists(x => relevantMembers[x] == MemberType.Service) && x.Exists(x => relevantMembers[x] == MemberType.Action))
            .ToList();
    }

    private static string BlockName(SyntaxNode block) =>
        block switch
        {
            AccessorDeclarationSyntax { Parent.Parent: PropertyDeclarationSyntax property } => property.GetName(),
            AccessorDeclarationSyntax { Parent.Parent: IndexerDeclarationSyntax } => UnspeakableIndexerName,
            ArrowExpressionClauseSyntax { Parent: PropertyDeclarationSyntax property } => property.GetName(),
            ArrowExpressionClauseSyntax { Parent: IndexerDeclarationSyntax } => UnspeakableIndexerName,
            MethodDeclarationSyntax method => method.GetName(),
            PropertyDeclarationSyntax property => property.GetName(),
            _ => null
        };

    private static ImmutableDictionary<string, MemberType> RelevantMembers(INamedTypeSymbol symbol)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, MemberType>();
        foreach (var member in symbol.GetMembers().Where(x => !x.IsStatic))
        {
            switch (member)
            {
                // Constructors are not considered because they have to be split anyway
                // Accessors are not considered because they are part of properties, that are considered as a whole
                case IMethodSymbol method when !method.IsConstructor() && method.MethodKind != MethodKind.StaticConstructor && method.AssociatedSymbol is not IPropertySymbol:
                    builder.Add(method.Name, MemberType.Action);
                    break;
                // Indexers are treated as methods with an unspeakable name
                case IPropertySymbol { IsIndexer: true }:
                    builder.Add(UnspeakableIndexerName, MemberType.Action);
                    break;
                // Primary constructor parameters may or may not generate fields, and must be considered
                case IMethodSymbol method when method.IsPrimaryConstructor():
                    builder.AddRange(method.Parameters.Where(IsService).Select(x => new KeyValuePair<string, MemberType>(x.Name, MemberType.Service)));
                    break;
                // Backing fields are excluded for auto-properties, since they are considered part of the property
                case IFieldSymbol field when IsService(field) && !field.IsImplicitlyDeclared:
                    builder.Add(field.Name, MemberType.Service);
                    break;
                case IPropertySymbol property when IsService(property):
                    builder.Add(property.Name, MemberType.Service);
                    break;
            }
        }

        return builder.ToImmutable();
    }

    private static bool IsService(ISymbol symbol) =>
        !ExcludedWellKnownServices.Contains(symbol.GetSymbolType().Name);

    private static IEnumerable<SecondaryLocation> SecondaryLocations(INamedTypeSymbol controllerSymbol, List<List<string>> sets)
    {
        for (var setIndex = 0; setIndex < sets.Count; setIndex++)
        {
            foreach (var memberLocation in sets[setIndex].SelectMany(MemberLocations))
            {
                yield return new SecondaryLocation(memberLocation, $"May belong to responsibility #{setIndex + 1}.");
            }
        }

        IEnumerable<Location> MemberLocations(string memberName) =>
            controllerSymbol.GetMembers(memberName).OfType<IMethodSymbol>().SelectMany(IdentifierLocations<MethodDeclarationSyntax>);
    }

    private static IEnumerable<Location> IdentifierLocations<T>(ISymbol symbol) where T : SyntaxNode =>
        symbol.DeclaringSyntaxReferences.Select(x => x.GetSyntax()).OfType<T>().Select(x => x.GetIdentifier()?.GetLocation());

    private record struct Dependency(string From, string To);
}
