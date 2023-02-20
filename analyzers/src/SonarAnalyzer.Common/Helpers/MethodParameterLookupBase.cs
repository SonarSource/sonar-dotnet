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

using System.Linq;

namespace SonarAnalyzer.Helpers;

public interface IMethodParameterLookup
{
    bool TryGetSymbol(SyntaxNode argument, out IParameterSymbol parameter);
    bool TryGetSyntax(IParameterSymbol parameter, out ImmutableArray<SyntaxNode> expressions);
    bool TryGetSyntax(string parameterName, out ImmutableArray<SyntaxNode> expressions);
    bool TryGetNonParamsSyntax(IParameterSymbol parameter, out SyntaxNode expression);
}

// This should come from the Roslyn API (https://github.com/dotnet/roslyn/issues/9)
internal abstract class MethodParameterLookupBase<TArgumentSyntax> : IMethodParameterLookup
    where TArgumentSyntax : SyntaxNode
{
    private readonly SeparatedSyntaxList<TArgumentSyntax>? argumentList;

    protected abstract SyntaxToken? GetNameColonArgumentIdentifier(TArgumentSyntax argument);
    protected abstract SyntaxNode Expression(TArgumentSyntax argument);

    public IMethodSymbol MethodSymbol { get; }
    private SymbolInfo? MethodSymbolInfo { get; }

    protected MethodParameterLookupBase(SeparatedSyntaxList<TArgumentSyntax>? argumentList, SymbolInfo? methodSymbolInfo)
        : this(argumentList, methodSymbolInfo?.Symbol as IMethodSymbol)
    {
        MethodSymbolInfo = methodSymbolInfo;
    }

    protected MethodParameterLookupBase(SeparatedSyntaxList<TArgumentSyntax>? argumentList, IMethodSymbol methodSymbol)
    {
        this.argumentList = argumentList;
        MethodSymbol = methodSymbol;
    }

    public bool TryGetSymbol(SyntaxNode argument, out IParameterSymbol parameter) => TryGetSymbol(argument, MethodSymbol, out parameter);

    private bool TryGetSymbol(SyntaxNode argument, IMethodSymbol methodSymbol, out IParameterSymbol parameter)
    {
        parameter = null;
        var arg = argument as TArgumentSyntax ?? throw new ArgumentException($"{nameof(argument)} must be of type {typeof(TArgumentSyntax)}", nameof(argument));

        if (!argumentList.HasValue
            || !argumentList.Value.Contains(arg)
            || methodSymbol == null
            || methodSymbol.IsVararg)
        {
            return false;
        }

        if (GetNameColonArgumentIdentifier(arg) is { } nameColonArgumentIdentifier)
        {
            parameter = methodSymbol.Parameters.FirstOrDefault(symbol => symbol.Name == nameColonArgumentIdentifier.ValueText);
            return parameter != null;
        }

        var index = argumentList.Value.IndexOf(arg);
        if (index >= methodSymbol.Parameters.Length)
        {
            var lastParameter = methodSymbol.Parameters.Last();
            parameter = lastParameter.IsParams ? lastParameter : null;
            return parameter != null;
        }
        parameter = methodSymbol.Parameters[index];
        return true;
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
        expressions = Enumerable.Empty<SyntaxNode>().ToImmutableArray();
        if (MethodSymbol is not null)
        {
            expressions = GetAllArgumentParameterMappings().Where(x => x.Symbol.Name == parameterName).Select(x => Expression(x.Node)).ToImmutableArray();
        }
        else if (MethodSymbolInfo is not null)
        {
            var candidateArgumentLists = MethodSymbolInfo.Value.CandidateSymbols.OfType<IMethodSymbol>()
                .Select(x => GetAllArgumentParameterMappings(x).Where(x => x.Symbol.Name == parameterName).Select(x => Expression(x.Node)).ToImmutableArray()).ToImmutableArray();
            if (candidateArgumentLists.Skip(1).All(x => x.SequenceEqual(candidateArgumentLists[0])))
            {
                expressions = candidateArgumentLists[0];
            }
        }
        return !expressions.IsEmpty;
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

    internal IEnumerable<NodeAndSymbol<TArgumentSyntax, IParameterSymbol>> GetAllArgumentParameterMappings() => GetAllArgumentParameterMappings(MethodSymbol);
    private IEnumerable<NodeAndSymbol<TArgumentSyntax, IParameterSymbol>> GetAllArgumentParameterMappings(IMethodSymbol methodSymbol)
    {
        if (argumentList.HasValue)
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
}
