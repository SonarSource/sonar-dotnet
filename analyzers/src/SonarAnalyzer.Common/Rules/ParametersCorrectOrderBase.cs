/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules;

public abstract class ParametersCorrectOrderBase<TSyntaxKind, TArgumentSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TArgumentSyntax : SyntaxNode
{
    protected const string DiagnosticId = "S2234";
    protected override string MessageFormat => "Parameters to '{0}' have the same names but not the same order as the method arguments.";

    private readonly DiagnosticDescriptor rule;

    protected abstract TypeInfo GetArgumentTypeSymbolInfo(TArgumentSyntax argument, SemanticModel model);
    protected abstract Location GetMethodDeclarationIdentifierLocation(SyntaxNode syntaxNode);
    protected abstract SyntaxToken? GetArgumentIdentifier(TArgumentSyntax argument, SemanticModel model);
    protected abstract SyntaxToken? GetNameColonArgumentIdentifier(TArgumentSyntax argument);

    protected ParametersCorrectOrderBase() : base(DiagnosticId) =>
        rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);

    internal void ReportIncorrectlyOrderedParameters(SonarSyntaxNodeReportingContext analysisContext,
        MethodParameterLookupBase<TArgumentSyntax> methodParameterLookup,
        SeparatedSyntaxList<TArgumentSyntax> argumentList,
        Func<Location> getLocationToReport)
    {
        var argumentParameterMappings = methodParameterLookup.GetAllArgumentParameterMappings()
            .ToDictionary(pair => pair.Node, pair => pair.Symbol);

        var methodSymbol = methodParameterLookup.MethodSymbol;
        if (methodSymbol == null)
        {
            return;
        }

        var parameterNames = argumentParameterMappings.Values
            .Select(symbol => symbol.Name.ToLowerInvariant())
            .Distinct()
            .ToList();

        var argumentIdentifiers = argumentList
            .Select(argument => ConvertToArgumentIdentifier(argument, analysisContext.SemanticModel))
            .ToList();
        var identifierNames = argumentIdentifiers
            .Select(p => p.IdentifierName?.ToLowerInvariant())
            .ToList();

        if (parameterNames.Intersect(identifierNames).Any() &&
            HasIncorrectlyOrderedParameters(argumentIdentifiers, argumentParameterMappings, parameterNames, identifierNames,
                analysisContext.SemanticModel))
        {
            // for VB the symbol does not contain the method syntax reference
            var secondaryLocations = methodSymbol.DeclaringSyntaxReferences
                .Select(s => GetMethodDeclarationIdentifierLocation(s.GetSyntax()))
                .WhereNotNull();

            analysisContext.ReportIssue(Diagnostic.Create(rule, getLocationToReport(), secondaryLocations, methodSymbol.Name));
        }
    }

    private bool HasIncorrectlyOrderedParameters(
        List<ArgumentIdentifier> argumentIdentifiers,
        Dictionary<TArgumentSyntax, IParameterSymbol> argumentParameterMappings,
        List<string> parameterNames,
        List<string> identifierNames, SemanticModel model)
    {
        var mappedParams = new HashSet<string>();
        var mappedArgs = new HashSet<string>();
        for (var i = 0; i < argumentIdentifiers.Count; i++)
        {
            var argumentIdentifier = argumentIdentifiers[i];
            var identifierName = argumentIdentifier.IdentifierName?.ToLowerInvariant();
            var parameter = argumentParameterMappings[argumentIdentifier.ArgumentSyntax];
            var parameterName = parameter.Name.ToLowerInvariant();

            if (string.IsNullOrEmpty(identifierName) || !parameterNames.Contains(identifierName))
            {
                continue;
            }

            if (argumentIdentifier is PositionalArgumentIdentifier positional &&
                (parameter.IsParams || identifierName == parameterName))
            {
                mappedParams.Add(parameterName);
                mappedArgs.Add(identifierName);
                continue;
            }

            if (mappedParams.Contains(parameterName)
                || mappedArgs.Contains(identifierName)
                || IdentifierWithSameNameAndTypeExistsLater(argumentIdentifier, i)
                || (!IdentifierWithSameNameAndTypeExists(parameter) && !ParameterWithSameNameAndTypeExists(argumentIdentifier))
                || (argumentIdentifier is NamedArgumentIdentifier named && (!identifierNames.Contains(named.DeclaredName) || named.DeclaredName == named.IdentifierName)))
            {
                continue;
            }
            return true;
        }
        return false;

        bool IdentifierWithSameNameAndTypeExists(IParameterSymbol parameter) =>
            argumentIdentifiers.Any(x =>
                x.IdentifierName == parameter.Name &&
                GetArgumentTypeSymbolInfo(x.ArgumentSyntax, model).ConvertedType.DerivesOrImplements(parameter.Type));

        bool IdentifierWithSameNameAndTypeExistsLater(ArgumentIdentifier argumentIdentifier, int index) =>
            argumentIdentifiers.Skip(index + 1)
                               .Any(x => string.Equals(x.IdentifierName, argumentIdentifier.IdentifierName, StringComparison.OrdinalIgnoreCase)
                                    && ArgumentTypesAreSame(x.ArgumentSyntax, argumentIdentifier.ArgumentSyntax));

        bool ArgumentTypesAreSame(TArgumentSyntax first, TArgumentSyntax second) =>
            GetArgumentTypeSymbolInfo(first, model).ConvertedType.DerivesOrImplements(GetArgumentTypeSymbolInfo(second, model).ConvertedType);

        bool ParameterWithSameNameAndTypeExists(ArgumentIdentifier argumentIdentifier) =>
            argumentParameterMappings.Values.Any(parameter => string.Equals(parameter.Name, argumentIdentifier.IdentifierName, StringComparison.OrdinalIgnoreCase) &&
                                                              GetArgumentTypeSymbolInfo(argumentIdentifier.ArgumentSyntax, model).ConvertedType.DerivesOrImplements(parameter.Type));
    }

    private ArgumentIdentifier ConvertToArgumentIdentifier(TArgumentSyntax argument, SemanticModel model)
    {
        var identifierName = GetArgumentIdentifier(argument, model)?.Text;
        var nameColonIdentifier = GetNameColonArgumentIdentifier(argument);

        return nameColonIdentifier == null
            ? new PositionalArgumentIdentifier(identifierName, argument)
            : new NamedArgumentIdentifier(identifierName, argument, nameColonIdentifier.Value.Text);
    }

    private class ArgumentIdentifier
    {
        protected ArgumentIdentifier(string identifierName, TArgumentSyntax argumentSyntax)
        {
            IdentifierName = identifierName;
            ArgumentSyntax = argumentSyntax;
        }

        public string IdentifierName { get; }
        public TArgumentSyntax ArgumentSyntax { get; }
    }

    private class PositionalArgumentIdentifier : ArgumentIdentifier
    {
        public PositionalArgumentIdentifier(string identifierName, TArgumentSyntax argumentSyntax)
            : base(identifierName, argumentSyntax)
        {
        }
    }

    private class NamedArgumentIdentifier : ArgumentIdentifier
    {
        public NamedArgumentIdentifier(string identifierName, TArgumentSyntax argumentSyntax, string declaredName)
            : base(identifierName, argumentSyntax)
        {
            DeclaredName = declaredName;
        }

        public string DeclaredName { get; }
    }
}
