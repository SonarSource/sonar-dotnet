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

using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Trackers;

public enum InvokedMemberKind
{
    Method,
    Constructor,
    Indexer,
    Attribute
}

public readonly struct ArgumentsCount
{
    public ArgumentsCount(int minimalNumberOfArguments, int maximalNumberOfArguments)
    {
        MinimalNumberOfArguments = minimalNumberOfArguments;
        MaximalNumberOfArguments = maximalNumberOfArguments;
    }

    public int MinimalNumberOfArguments { get; }
    public int MaximalNumberOfArguments { get; }

    public static implicit operator ArgumentsCount(int exact) => new(exact, exact);
}

public class ArgumentDescriptor
{
    private ArgumentDescriptor(InvokedMemberKind memberKind, Func<IReadOnlyCollection<SyntaxNode>, bool> argumentListConstraint, RefKind? refKind,
        Func<IParameterSymbol, bool> parameterConstraint, Func<string, StringComparison, bool> invokedMemberNameConstraint, Func<ISymbol, bool> invokedMemberConstraint)
    {
        MemberKind = memberKind;
        ArgumentListConstraint = argumentListConstraint;
        RefKind = refKind;
        ParameterConstraint = parameterConstraint;
        InvokedMemberNameConstraint = invokedMemberNameConstraint;
        InvokedMemberConstraint = invokedMemberConstraint;
    }

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, string parameterName, int numberOfArguments)
        => MethodInvocation(invokedType, methodName, parameterName, (ArgumentsCount)numberOfArguments);

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, string parameterName, ArgumentsCount numberOfArguments)
        => MethodInvocation(s => invokedType.Matches(s.ContainingType), methodName, parameterName, numberOfArguments, null);

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, string parameterName, ArgumentsCount numberOfArguments, RefKind refKind)
        => MethodInvocation(s => invokedType.Matches(s.ContainingType), methodName, parameterName, numberOfArguments, refKind);

    public static ArgumentDescriptor MethodInvocation(Func<IMethodSymbol, bool> invokedMethodSymbol, string methodName, string parameterName, ArgumentsCount argumentsCount, RefKind? refKind)
        => new(InvokedMemberKind.Method,
            argumentListConstraint: x => x.Count >= argumentsCount.MinimalNumberOfArguments && x.Count <= argumentsCount.MaximalNumberOfArguments,
            refKind,
            parameterConstraint: x => x.Name == parameterName,
            invokedMemberNameConstraint: (n, c) => n.Equals(methodName, c),
            invokedMemberConstraint: x => invokedMethodSymbol(x as IMethodSymbol));

    public InvokedMemberKind MemberKind { get; }
    public Func<IReadOnlyCollection<SyntaxNode>, bool> ArgumentListConstraint { get; }
    public RefKind? RefKind { get; }
    public Func<IParameterSymbol, bool> ParameterConstraint { get; }
    public Func<string, StringComparison, bool> InvokedMemberNameConstraint { get; }
    public Func<ISymbol, bool> InvokedMemberConstraint { get; }
}
