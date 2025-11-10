/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Syntax.Utilities;

public interface IMethodParameterLookup
{
    IMethodSymbol MethodSymbol { get; }
    bool TryGetSymbol(SyntaxNode argument, out IParameterSymbol parameter);
    bool TryGetSyntax(IParameterSymbol parameter, out ImmutableArray<SyntaxNode> expressions);
    bool TryGetSyntax(string parameterName, out ImmutableArray<SyntaxNode> expressions);
    bool TryGetNonParamsSyntax(IParameterSymbol parameter, out SyntaxNode expression);
}

// This should come from the Roslyn API (https://github.com/dotnet/roslyn/issues/9)
public abstract class MethodParameterLookupBase<TArgumentSyntax> : IMethodParameterLookup
    where TArgumentSyntax : SyntaxNode
{
    private readonly SeparatedSyntaxList<TArgumentSyntax> argumentList;

    protected abstract SyntaxToken? GetNameColonIdentifier(TArgumentSyntax argument);
    protected abstract SyntaxToken? GetNameEqualsIdentifier(TArgumentSyntax argument);
    protected abstract SyntaxNode Expression(TArgumentSyntax argument);

    public IMethodSymbol MethodSymbol { get; }
    private ImmutableArray<IMethodSymbol> MethodSymbolOrCandidates { get; }

    protected MethodParameterLookupBase(SeparatedSyntaxList<TArgumentSyntax> argumentList, SymbolInfo? methodSymbolInfo)
        : this(argumentList, methodSymbolInfo?.Symbol as IMethodSymbol, methodSymbolInfo?.AllSymbols().OfType<IMethodSymbol>()) { }

    protected MethodParameterLookupBase(SeparatedSyntaxList<TArgumentSyntax> argumentList, IMethodSymbol methodSymbol)
        : this(argumentList, methodSymbol, [methodSymbol]) { }

    private MethodParameterLookupBase(SeparatedSyntaxList<TArgumentSyntax> argumentList, IMethodSymbol methodSymbol, IEnumerable<IMethodSymbol> methodSymbolOrCandidates)
    {
        this.argumentList = argumentList;
        MethodSymbol = methodSymbol;
        MethodSymbolOrCandidates = methodSymbolOrCandidates?.ToImmutableArray() ?? ImmutableArray.Create<IMethodSymbol>();
    }

    /// <summary>
    /// Method returns array of argument syntaxes that represents all syntaxes passed to the parameter.
    ///
    /// There could be multiple syntaxes for ParamArray/params.
    /// There could be zero or one result for optional parameters.
    /// There will be single result for normal parameters.
    /// </summary>
    public bool TryGetSyntax(IParameterSymbol parameter, out ImmutableArray<SyntaxNode> expressions) =>
        TryGetSyntax(parameter.Name, out expressions);

    /// <summary>
    /// Method returns array of argument syntaxes that represents all syntaxes passed to the parameter.
    ///
    /// There could be multiple syntaxes for ParamArray/params.
    /// There could be zero or one result for optional parameters.
    /// There will be single result for normal parameters.
    public bool TryGetSyntax(string parameterName, out ImmutableArray<SyntaxNode> expressions)
    {
        var candidateArgumentLists = MethodSymbolOrCandidates
            .Select(x => GetAllArgumentParameterMappings(x).Where(x => x.Symbol.Name == parameterName).Select(x => Expression(x.Node)).ToImmutableArray()).ToImmutableArray();
        expressions = candidateArgumentLists.Any() && AllArgumentsAreTheSame(candidateArgumentLists)
            ? candidateArgumentLists[0]
            : Enumerable.Empty<SyntaxNode>().ToImmutableArray();
        return !expressions.IsEmpty;

        static bool AllArgumentsAreTheSame(ImmutableArray<ImmutableArray<SyntaxNode>> candidateArgumentLists) =>
            candidateArgumentLists.Skip(1).All(x => x.SequenceEqual(candidateArgumentLists[0]));
    }

    /// <summary>
    /// Method returns zero or one argument syntax that represents syntax passed to the parameter.
    ///
    /// Caller must ensure that given parameter is not ParamArray/params.
    /// </summary>
    public bool TryGetNonParamsSyntax(IParameterSymbol parameter, out SyntaxNode expression)
    {
        if (parameter.IsParams)
        {
            throw new InvalidOperationException("Cannot call TryGetNonParamsSyntax on ParamArray/params parameters.");
        }
        if (TryGetSyntax(parameter, out var all))
        {
            expression = all.Single();
            return true;
        }
        expression = null;
        return false;
    }

    public IEnumerable<NodeAndSymbol<TArgumentSyntax, IParameterSymbol>> GetAllArgumentParameterMappings() =>
        GetAllArgumentParameterMappings(MethodSymbol);

    public bool TryGetSymbol(SyntaxNode argument, out IParameterSymbol parameter) =>
        TryGetSymbol(argument, MethodSymbol, out parameter);

    private bool TryGetSymbol(SyntaxNode argument, IMethodSymbol methodSymbol, out IParameterSymbol parameter)
    {
        parameter = null;
        var arg = argument as TArgumentSyntax ?? throw new ArgumentException($"{nameof(argument)} must be of type {typeof(TArgumentSyntax)}", nameof(argument));

        if (!argumentList.Contains(arg)
            || methodSymbol is null
            || methodSymbol.IsVararg)
        {
            return false;
        }

        if (GetNameColonIdentifier(arg) is { } nameColonIdentifier)
        {
            parameter = methodSymbol.Parameters.FirstOrDefault(x => x.Name == nameColonIdentifier.ValueText);
            return parameter is not null;
        }

        if (GetNameEqualsIdentifier(arg) is { } nameEqualsIdentifier
            && methodSymbol.ContainingType.GetMembers(nameEqualsIdentifier.ValueText) is { Length: 1 } properties
            && properties[0] is IPropertySymbol { SetMethod: { } setter } property
            && property.Name == nameEqualsIdentifier.ValueText
            && setter.Parameters is { Length: 1 } parameters)
        {
            parameter = parameters[0];
            return parameter is not null;
        }

        var index = argumentList.IndexOf(arg);
        if (index >= methodSymbol.Parameters.Length)
        {
            var lastParameter = methodSymbol.Parameters.Last();
            parameter = lastParameter.IsParams ? lastParameter : null;
            return parameter is not null;
        }
        parameter = methodSymbol.Parameters[index];
        return true;
    }

    private IEnumerable<NodeAndSymbol<TArgumentSyntax, IParameterSymbol>> GetAllArgumentParameterMappings(IMethodSymbol methodSymbol)
    {
        foreach (var argument in argumentList)
        {
            if (TryGetSymbol(argument, methodSymbol, out var parameter))
            {
                yield return new NodeAndSymbol<TArgumentSyntax, IParameterSymbol>(argument, parameter);
            }
        }
    }
}
