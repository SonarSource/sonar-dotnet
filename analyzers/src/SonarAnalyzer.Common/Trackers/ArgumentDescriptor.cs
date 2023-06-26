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

namespace SonarAnalyzer.Trackers;

public enum InvokedMemberKind
{
    Method,
    Constructor,
    Indexer,
    Attribute
}

public class ArgumentDescriptor
{
    private ArgumentDescriptor(InvokedMemberKind memberKind, Func<ISymbol, bool> invokedMemberConstraint, Func<string, StringComparison, bool> invokedMemberNameConstraint,
        Func<IReadOnlyCollection<SyntaxNode>, int?, bool> argumentListConstraint, Func<IParameterSymbol, bool> parameterConstraint, RefKind? refKind)
    {
        MemberKind = memberKind;
        ArgumentListConstraint = argumentListConstraint;
        RefKind = refKind;
        ParameterConstraint = parameterConstraint;
        InvokedMemberNameConstraint = invokedMemberNameConstraint;
        InvokedMemberConstraint = invokedMemberConstraint;
    }

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, string parameterName, int argumentPosition)
        => MethodInvocation(invokedType, methodName, parameterName, x => x == argumentPosition);

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, string parameterName, Func<int, bool> argumentPosition)
        => MethodInvocation(invokedType, methodName, p => p.Name == parameterName, argumentPosition, null);

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, string parameterName, Func<int, bool> argumentPosition, RefKind refKind)
        => MethodInvocation(invokedType, methodName, p => p.Name == parameterName, argumentPosition, refKind);

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, Func<IParameterSymbol, bool> parameterConstraint, Func<int, bool> argumentPosition, RefKind? refKind)
        => MethodInvocation(invokedType, (n, c) => n.Equals(methodName, c), parameterConstraint, argumentPosition, refKind);

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, Func<string, StringComparison, bool> invokedMemberNameConstraint, Func<IParameterSymbol, bool> parameterConstraint,
        Func<int, bool> argumentPosition, RefKind? refKind)
        => MethodInvocation(s => invokedType.Matches(s.ContainingType), invokedMemberNameConstraint, parameterConstraint, argumentPosition, refKind);

    public static ArgumentDescriptor MethodInvocation(Func<IMethodSymbol, bool> invokedMethodSymbol, Func<string, StringComparison, bool> invokedMemberNameConstraint,
        Func<IParameterSymbol, bool> parameterConstraint, Func<int, bool> argumentPosition, RefKind? refKind)
        => MethodInvocation(invokedMethodSymbol,
            invokedMemberNameConstraint,
            parameterConstraint,
            (_, position) => position is null || argumentPosition is null || argumentPosition(position.Value),
            refKind);

    public static ArgumentDescriptor MethodInvocation(Func<IMethodSymbol, bool> invokedMethodSymbol, Func<string, StringComparison, bool> invokedMemberNameConstraint,
        Func<IParameterSymbol, bool> parameterConstraint, Func<IReadOnlyCollection<SyntaxNode>, int?, bool> argumentListConstraint, RefKind? refKind)
        => new(InvokedMemberKind.Method,
            invokedMemberConstraint: x => invokedMethodSymbol(x as IMethodSymbol),
            invokedMemberNameConstraint: invokedMemberNameConstraint,
            argumentListConstraint: argumentListConstraint,
            parameterConstraint: parameterConstraint,
            refKind: refKind);

    public InvokedMemberKind MemberKind { get; }
    public Func<IReadOnlyCollection<SyntaxNode>, int?, bool> ArgumentListConstraint { get; }
    public RefKind? RefKind { get; }
    public Func<IParameterSymbol, bool> ParameterConstraint { get; }
    public Func<string, StringComparison, bool> InvokedMemberNameConstraint { get; }
    public Func<ISymbol, bool> InvokedMemberConstraint { get; }
}
