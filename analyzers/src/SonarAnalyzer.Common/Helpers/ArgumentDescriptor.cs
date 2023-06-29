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

namespace SonarAnalyzer.Helpers;

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

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, string parameterName, int argumentPosition) =>
        MethodInvocation(invokedType, methodName, parameterName, x => x == argumentPosition);

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, string parameterName, Func<int, bool> argumentPosition) =>
        MethodInvocation(invokedType, methodName, p => p.Name == parameterName, argumentPosition, null);

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, string parameterName, Func<int, bool> argumentPosition, RefKind refKind) =>
        MethodInvocation(invokedType, methodName, p => p.Name == parameterName, argumentPosition, refKind);

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, Func<IParameterSymbol, bool> parameterConstraint, Func<int, bool> argumentPosition, RefKind? refKind) =>
        MethodInvocation(invokedType, (n, c) => n.Equals(methodName, c), parameterConstraint, argumentPosition, refKind);

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, Func<string, StringComparison, bool> invokedMemberNameConstraint, Func<IParameterSymbol, bool> parameterConstraint,
        Func<int, bool> argumentPosition, RefKind? refKind) =>
        MethodInvocation(s => invokedType.Matches(s.ContainingType), invokedMemberNameConstraint, parameterConstraint, argumentPosition, refKind);

    public static ArgumentDescriptor MethodInvocation(Func<IMethodSymbol, bool> invokedMethodSymbol, Func<string, StringComparison, bool> invokedMemberNameConstraint,
        Func<IParameterSymbol, bool> parameterConstraint, Func<int, bool> argumentPosition, RefKind? refKind) =>
        MethodInvocation(invokedMethodSymbol,
            invokedMemberNameConstraint,
            parameterConstraint,
            (_, position) => position is null || argumentPosition is null || argumentPosition(position.Value),
            refKind);

    public static ArgumentDescriptor MethodInvocation(Func<IMethodSymbol, bool> invokedMethodSymbol, Func<string, StringComparison, bool> invokedMemberNameConstraint,
        Func<IParameterSymbol, bool> parameterConstraint, Func<IReadOnlyCollection<SyntaxNode>, int?, bool> argumentListConstraint, RefKind? refKind) =>
        new(InvokedMemberKind.Method,
            invokedMemberConstraint: x => invokedMethodSymbol(x as IMethodSymbol),
            invokedMemberNameConstraint: invokedMemberNameConstraint,
            argumentListConstraint: argumentListConstraint,
            parameterConstraint: parameterConstraint,
            refKind: refKind);

    public static ArgumentDescriptor ConstructorInvocation(KnownType constructedType, string parameterName, int argumentPosition) =>
        ConstructorInvocation(
            x => constructedType.Matches(x.ContainingType),
            (x, c) => x.Equals(constructedType.TypeName, c),
            x => x.Name == parameterName,
            (_, x) => x is null || x == argumentPosition,
            null);

    public static ArgumentDescriptor ConstructorInvocation(Func<IMethodSymbol, bool> invokedMethodSymbol, Func<string, StringComparison, bool> invokedMemberNameConstraint,
        Func<IParameterSymbol, bool> parameterConstraint, Func<IReadOnlyCollection<SyntaxNode>, int?, bool> argumentListConstraint, RefKind? refKind) =>
        new(InvokedMemberKind.Constructor,
            invokedMemberConstraint: x => invokedMethodSymbol(x as IMethodSymbol),
            invokedMemberNameConstraint: invokedMemberNameConstraint,
            argumentListConstraint: argumentListConstraint,
            parameterConstraint: parameterConstraint,
            refKind: refKind);

    public static ArgumentDescriptor ElementAccess(KnownType invokedIndexerContainer, Func<IParameterSymbol, bool> parameterConstraint, int argumentPosition) =>
        ElementAccess(
            invokedIndexerContainer,
            null,
            parameterConstraint,
            x => x == argumentPosition);

    public static ArgumentDescriptor ElementAccess(KnownType invokedIndexerContainer, string invokedIndexerExpression, Func<IParameterSymbol, bool> parameterConstraint, int argumentPosition) =>
        ElementAccess(invokedIndexerContainer, invokedIndexerExpression, parameterConstraint, x => x == argumentPosition);

    public static ArgumentDescriptor ElementAccess(KnownType invokedIndexerContainer,
        Func<IParameterSymbol, bool> parameterConstraint, Func<int, bool> argumentPositionConstraint) =>
        ElementAccess(
            invokedIndexerContainer,
            null,
            parameterConstraint,
            argumentPositionConstraint);

    public static ArgumentDescriptor ElementAccess(KnownType invokedIndexerContainer, string invokedIndexerExpression,
        Func<IParameterSymbol, bool> parameterConstraint, Func<int, bool> argumentPositionConstraint) =>
        ElementAccess(
            x => x is { ContainingSymbol: INamedTypeSymbol { } container } && invokedIndexerContainer.Matches(container),
            (s, c) => invokedIndexerExpression is null || s.Equals(invokedIndexerExpression, c),
            argumentListConstraint: (_, p) => argumentPositionConstraint is null || p is null || argumentPositionConstraint(p.Value),
            parameterConstraint: parameterConstraint);

    public static ArgumentDescriptor ElementAccess(Func<IMethodSymbol, bool> invokedIndexerPropertyMethod, Func<string, StringComparison, bool> invokedIndexerExpression,
        Func<IParameterSymbol, bool> parameterConstraint, Func<IReadOnlyCollection<SyntaxNode>, int?, bool> argumentListConstraint) =>
        new(InvokedMemberKind.Indexer,
            invokedMemberConstraint: x => invokedIndexerPropertyMethod(x as IMethodSymbol),
            invokedMemberNameConstraint: invokedIndexerExpression,
            argumentListConstraint: argumentListConstraint,
            parameterConstraint: parameterConstraint,
            refKind: null);

    public static ArgumentDescriptor AttributeArgument(string attributeName, string parameterName, int argumentPosition) =>
        AttributeArgument(
            x => x is { MethodKind: MethodKind.Constructor, ContainingType.Name: { } name } && (name == attributeName || name == $"{attributeName}Attribute"),
            (x, c) => AttributeClassNameConstraint(attributeName, x, c),
            p => p.Name == parameterName,
            (_, i) => i is null || i.Value == argumentPosition);

    public static ArgumentDescriptor AttributeProperty(string attributeName, string propertyName) =>
        AttributeArgument(
            attributeConstructorConstraint: x => x is { MethodKind: MethodKind.PropertySet, AssociatedSymbol.Name: { } name } && name == propertyName,
            attributeNameConstraint: (s, c) => AttributeClassNameConstraint(attributeName, s, c),
            parameterConstraint: p => true,
            argumentListConstraint: (l, i) => true);

    public static ArgumentDescriptor AttributeArgument(Func<IMethodSymbol, bool> attributeConstructorConstraint, Func<string, StringComparison, bool> attributeNameConstraint,
        Func<IParameterSymbol, bool> parameterConstraint, Func<IReadOnlyCollection<SyntaxNode>, int?, bool> argumentListConstraint) =>
        new(InvokedMemberKind.Attribute,
            invokedMemberConstraint: x => attributeConstructorConstraint(x as IMethodSymbol),
            invokedMemberNameConstraint: attributeNameConstraint,
            argumentListConstraint: argumentListConstraint,
            parameterConstraint: parameterConstraint,
            refKind: null);

    private static bool AttributeClassNameConstraint(string expectedAttributeName, string nodeClassName, StringComparison c) =>
        nodeClassName.Equals(expectedAttributeName, c) || nodeClassName.Equals($"{expectedAttributeName}Attribute");

    public InvokedMemberKind MemberKind { get; }
    public Func<IReadOnlyCollection<SyntaxNode>, int?, bool> ArgumentListConstraint { get; }
    public RefKind? RefKind { get; }
    public Func<IParameterSymbol, bool> ParameterConstraint { get; }
    public Func<string, StringComparison, bool> InvokedMemberNameConstraint { get; }
    public Func<ISymbol, bool> InvokedMemberConstraint { get; }
}
