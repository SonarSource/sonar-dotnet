﻿/*
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

    protected abstract TypeInfo ArgumentType(TArgumentSyntax argument, SemanticModel model);
    protected abstract Location MethodDeclarationIdentifierLocation(SyntaxNode syntaxNode);
    protected abstract SyntaxToken? GetArgumentIdentifier(TArgumentSyntax argument, SemanticModel model);
    protected abstract SyntaxToken? NameColonArgumentIdentifier(TArgumentSyntax argument);
    protected abstract Location GetLocation(SyntaxNode node);

    protected ParametersCorrectOrderBase() : base(DiagnosticId) =>
        rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);

    internal void ReportIncorrectlyOrderedParameters(SonarSyntaxNodeReportingContext analysisContext,
                                                     MethodParameterLookupBase<TArgumentSyntax> methodParameterLookup,
                                                     SeparatedSyntaxList<TArgumentSyntax> argumentList,
                                                     SyntaxNode reportingNode)
    {
        var argumentParameterMappings = methodParameterLookup.GetAllArgumentParameterMappings().ToDictionary(x => x.Node, x => x.Symbol);
        if (methodParameterLookup.MethodSymbol is not { } methodSymbol)
        {
            return;
        }

        var parameterNames = argumentParameterMappings.Values.Select(x => x.Name.ToUpperInvariant()).Distinct().ToList();
        var argumentIdentifiers = argumentList.Select(x => ConvertToArgumentIdentifier(x, analysisContext.SemanticModel)).ToList();
        var identifierNames = argumentIdentifiers.Select(x => x.IdentifierName?.ToUpperInvariant()).ToList();

        if (parameterNames.Intersect(identifierNames).Any()
            && HasIncorrectlyOrderedParameters(argumentIdentifiers, argumentParameterMappings, parameterNames, identifierNames, analysisContext.SemanticModel))
        {
            // for VB the symbol does not contain the method syntax reference
            var secondaryLocations = methodParameterLookup.MethodSymbol.DeclaringSyntaxReferences.Select(x => MethodDeclarationIdentifierLocation(x.GetSyntax())).WhereNotNull();
            analysisContext.ReportIssue(Diagnostic.Create(rule, GetLocation(reportingNode), secondaryLocations, methodSymbol.Name));
        }
    }

    private bool HasIncorrectlyOrderedParameters(List<ArgumentIdentifier> argumentIdentifiers,
                                                 Dictionary<TArgumentSyntax, IParameterSymbol> argumentParameterMappings,
                                                 List<string> parameterNames,
                                                 List<string> identifierNames, SemanticModel model)
    {
        var mappedParams = new HashSet<string>();
        var mappedArgs = new HashSet<string>();
        for (var i = 0; i < argumentIdentifiers.Count; i++)
        {
            var argumentIdentifier = argumentIdentifiers[i];
            var identifierName = argumentIdentifier.IdentifierName?.ToUpperInvariant();
            var parameter = argumentParameterMappings[argumentIdentifier.ArgumentSyntax];
            var parameterName = parameter.Name.ToUpperInvariant();

            if (!string.IsNullOrEmpty(identifierName) && parameterNames.Contains(identifierName))
            {
                if (argumentIdentifier is PositionalArgumentIdentifier && (parameter.IsParams || identifierName == parameterName))
                {
                    mappedParams.Add(parameterName);
                    mappedArgs.Add(identifierName);
                }
                else if (!mappedParams.Contains(parameterName)
                    && !mappedArgs.Contains(identifierName)
                    && !IdentifierWithSameNameAndTypeExistsLater(argumentIdentifier, i)
                    && !(!IdentifierWithSameNameAndTypeExists(parameter) && !ParameterWithSameNameAndTypeExists(argumentIdentifier))
                    && !IdentifierMatchesDeclaredName(argumentIdentifier))
                {
                    return true;
                }
            }
        }
        return false;

        bool IdentifierWithSameNameAndTypeExists(IParameterSymbol parameter) =>
            argumentIdentifiers.Exists(x => x.IdentifierName == parameter.Name
                                            && ArgumentType(x.ArgumentSyntax, model).ConvertedType.DerivesOrImplements(parameter.Type));

        bool IdentifierWithSameNameAndTypeExistsLater(ArgumentIdentifier argumentIdentifier, int index) =>
            argumentIdentifiers.Skip(index + 1)
                               .Any(x => string.Equals(x.IdentifierName, argumentIdentifier.IdentifierName, StringComparison.OrdinalIgnoreCase)
                                         && ArgumentTypesAreSame(x.ArgumentSyntax, argumentIdentifier.ArgumentSyntax));

        bool ArgumentTypesAreSame(TArgumentSyntax first, TArgumentSyntax second) =>
            ArgumentType(first, model).ConvertedType.DerivesOrImplements(ArgumentType(second, model).ConvertedType);

        bool ParameterWithSameNameAndTypeExists(ArgumentIdentifier argumentIdentifier) =>
            argumentParameterMappings.Values.Any(x => string.Equals(x.Name, argumentIdentifier.IdentifierName, StringComparison.OrdinalIgnoreCase)
                                                      && ArgumentType(argumentIdentifier.ArgumentSyntax, model).ConvertedType.DerivesOrImplements(x.Type));

        bool IdentifierMatchesDeclaredName(ArgumentIdentifier argumentIdentifier) =>
            argumentIdentifier is NamedArgumentIdentifier named
            && (!identifierNames.Contains(named.DeclaredName.ToUpperInvariant()) || named.DeclaredName == named.IdentifierName);
    }

    private ArgumentIdentifier ConvertToArgumentIdentifier(TArgumentSyntax argument, SemanticModel model)
    {
        var identifierName = GetArgumentIdentifier(argument, model)?.Text;
        return NameColonArgumentIdentifier(argument) is { } nameColonIdentifier
            ? new NamedArgumentIdentifier(identifierName, argument, nameColonIdentifier.Value.ToString())
            : new PositionalArgumentIdentifier(identifierName, argument);
    }

    private class ArgumentIdentifier
    {
        public string IdentifierName { get; }
        public TArgumentSyntax ArgumentSyntax { get; }
        protected ArgumentIdentifier(string identifierName, TArgumentSyntax argumentSyntax)
        {
            IdentifierName = identifierName;
            ArgumentSyntax = argumentSyntax;
        }
    }

    private sealed class PositionalArgumentIdentifier : ArgumentIdentifier
    {
        public PositionalArgumentIdentifier(string identifierName, TArgumentSyntax argumentSyntax) : base(identifierName, argumentSyntax) { }
    }

    private sealed class NamedArgumentIdentifier : ArgumentIdentifier
    {
        public string DeclaredName { get; }
        public NamedArgumentIdentifier(string identifierName, TArgumentSyntax argumentSyntax, string declaredName) : base(identifierName, argumentSyntax) =>
            DeclaredName = declaredName;
    }
}
