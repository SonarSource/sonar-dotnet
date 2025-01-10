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

namespace SonarAnalyzer.Core.Trackers;

public enum MemberKind
{
    Method,
    Constructor,
    Indexer,
    Attribute
}

public class ArgumentDescriptor
{
    public MemberKind MemberKind { get; }
    public Func<IReadOnlyCollection<SyntaxNode>, int?, bool> ArgumentListConstraint { get; }
    public RefKind? RefKind { get; }
    public Func<IParameterSymbol, bool> ParameterConstraint { get; }
    public Func<string, StringComparison, bool> InvokedMemberNameConstraint { get; }
    public Func<SemanticModel, ILanguageFacade, SyntaxNode, bool> InvokedMemberNodeConstraint { get; }
    public Func<IMethodSymbol, bool> InvokedMemberConstraint { get; }

    private ArgumentDescriptor(MemberKind memberKind,
                               Func<IMethodSymbol, bool> invokedMemberConstraint,
                               Func<string, StringComparison, bool> invokedMemberNameConstraint,
                               Func<SemanticModel, ILanguageFacade, SyntaxNode, bool> invokedMemberNodeConstraint,
                               Func<IReadOnlyCollection<SyntaxNode>, int?, bool> argumentListConstraint,
                               Func<IParameterSymbol, bool> parameterConstraint,
                               RefKind? refKind)
    {
        MemberKind = memberKind;
        ArgumentListConstraint = argumentListConstraint;
        RefKind = refKind;
        ParameterConstraint = parameterConstraint;
        InvokedMemberNameConstraint = invokedMemberNameConstraint;
        InvokedMemberNodeConstraint = invokedMemberNodeConstraint;
        InvokedMemberConstraint = invokedMemberConstraint;
    }

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, string parameterName, int argumentPosition) =>
        MethodInvocation(invokedType, methodName, parameterName, x => x == argumentPosition);

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, string parameterName, Func<int, bool> argumentPosition) =>
        MethodInvocation(invokedType, methodName, x => x.Name == parameterName, argumentPosition, null);

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType, string methodName, string parameterName, Func<int, bool> argumentPosition, RefKind refKind) =>
        MethodInvocation(invokedType, methodName, x => x.Name == parameterName, argumentPosition, refKind);

    public static ArgumentDescriptor MethodInvocation(KnownType invokedType,
                                                      Func<string, StringComparison, bool> invokedMemberNameConstraint,
                                                      Func<IParameterSymbol, bool> parameterConstraint,
                                                      Func<int, bool> argumentPosition,
                                                      RefKind? refKind) =>
        MethodInvocation(x => invokedType.Matches(x.ContainingType), invokedMemberNameConstraint, parameterConstraint, argumentPosition, refKind);

    public static ArgumentDescriptor MethodInvocation(Func<IMethodSymbol, bool> invokedMemberConstraint,
                                                      Func<string, StringComparison, bool> invokedMemberNameConstraint,
                                                      Func<IParameterSymbol, bool> parameterConstraint,
                                                      Func<int, bool> argumentPosition,
                                                      RefKind? refKind) =>
        MethodInvocation(invokedMemberConstraint,
            invokedMemberNameConstraint,
            (_, _, _) => true,
            parameterConstraint,
            (_, position) => position is null || argumentPosition is null || argumentPosition(position.Value),
            refKind);

    public static ArgumentDescriptor MethodInvocation(Func<IMethodSymbol, bool> invokedMemberConstraint,
                                                      Func<string, StringComparison, bool> invokedMemberNameConstraint,
                                                      Func<SemanticModel, ILanguageFacade, SyntaxNode, bool> invokedMemberNodeConstraint,
                                                      Func<IParameterSymbol, bool> parameterConstraint,
                                                      Func<IReadOnlyCollection<SyntaxNode>, int?, bool> argumentListConstraint,
                                                      RefKind? refKind) =>
        new(MemberKind.Method, invokedMemberConstraint, invokedMemberNameConstraint, invokedMemberNodeConstraint, argumentListConstraint, parameterConstraint, refKind);

    public static ArgumentDescriptor ConstructorInvocation(KnownType constructedType, string parameterName, int argumentPosition) =>
        ConstructorInvocation(
            x => constructedType.Matches(x.ContainingType),
            (x, c) => x.Equals(constructedType.TypeName, c),
            static (_, _, _) => true,
            x => x.Name == parameterName,
            (_, x) => x is null || x == argumentPosition,
            null);

    public static ArgumentDescriptor ConstructorInvocation(Func<IMethodSymbol, bool> invokedMethodSymbol,
                                                           Func<string, StringComparison, bool> invokedMemberNameConstraint,
                                                           Func<SemanticModel, ILanguageFacade, SyntaxNode, bool> invokedMemberNodeConstraint,
                                                           Func<IParameterSymbol, bool> parameterConstraint,
                                                           Func<IReadOnlyCollection<SyntaxNode>, int?, bool> argumentListConstraint,
                                                           RefKind? refKind) =>
        new(MemberKind.Constructor,
            invokedMemberConstraint: invokedMethodSymbol,
            invokedMemberNameConstraint,
            invokedMemberNodeConstraint,
            argumentListConstraint,
            parameterConstraint,
            refKind);

    public static ArgumentDescriptor ElementAccess(KnownType invokedIndexerContainer, Func<IParameterSymbol, bool> parameterConstraint, int argumentPosition) =>
        ElementAccess(invokedIndexerContainer, null, parameterConstraint, x => x == argumentPosition);

    public static ArgumentDescriptor ElementAccess(KnownType invokedIndexerContainer, string invokedIndexerExpression, Func<IParameterSymbol, bool> parameterConstraint, int argumentPosition) =>
        ElementAccess(invokedIndexerContainer, invokedIndexerExpression, parameterConstraint, x => x == argumentPosition);

    public static ArgumentDescriptor ElementAccess(KnownType invokedIndexerContainer, Func<IParameterSymbol, bool> parameterConstraint, Func<int, bool> argumentPositionConstraint) =>
        ElementAccess(invokedIndexerContainer, null, parameterConstraint, argumentPositionConstraint);

    public static ArgumentDescriptor ElementAccess(KnownType invokedIndexerContainer,
                                                   string invokedIndexerExpression,
                                                   Func<IParameterSymbol, bool> parameterConstraint,
                                                   Func<int, bool> argumentPositionConstraint) =>

        ElementAccess(
            x => x is { ContainingSymbol: INamedTypeSymbol container } && invokedIndexerContainer.Matches(container),
            (s, c) => invokedIndexerExpression is null || s.Equals(invokedIndexerExpression, c),
            (_, _, _) => true,
            parameterConstraint,
            (_, p) => argumentPositionConstraint is null || p is null || argumentPositionConstraint(p.Value));

    public static ArgumentDescriptor ElementAccess(Func<IMethodSymbol, bool> invokedIndexerPropertyMethod,
                                                   Func<string, StringComparison, bool> invokedIndexerExpression,
                                                   Func<SemanticModel, ILanguageFacade, SyntaxNode, bool> invokedIndexerExpressionNodeConstraint,
                                                   Func<IParameterSymbol, bool> parameterConstraint,
                                                   Func<IReadOnlyCollection<SyntaxNode>, int?, bool> argumentListConstraint) =>
        new(MemberKind.Indexer,
            invokedMemberConstraint: invokedIndexerPropertyMethod,
            invokedMemberNameConstraint: invokedIndexerExpression,
            invokedMemberNodeConstraint: invokedIndexerExpressionNodeConstraint,
            argumentListConstraint: argumentListConstraint,
            parameterConstraint: parameterConstraint,
            refKind: null);

    public static ArgumentDescriptor AttributeArgument(KnownType attributeType, string constructorParameterName, int argumentPosition) =>
        AttributeArgument(
            x => x is { MethodKind: MethodKind.Constructor, ContainingType: { Name: { } name } type }
                && attributeType.Matches(type)
                && AttributeClassNameConstraint(attributeType.TypeName, name, StringComparison.Ordinal),
            (x, c) => AttributeClassNameConstraint(attributeType.TypeName, x, c),
            (_, _, _) => true,
            x => x.Name == constructorParameterName,
            (_, i) => i is null || i.Value == argumentPosition);

    public static ArgumentDescriptor AttributeArgument(string attributeName, string parameterName, int argumentPosition) =>
        AttributeArgument(
            x => x is { MethodKind: MethodKind.Constructor, ContainingType.Name: { } name } && AttributeClassNameConstraint(attributeName, name, StringComparison.Ordinal),
            (x, c) => AttributeClassNameConstraint(attributeName, x, c),
            (_, _, _) => true,
            x => x.Name == parameterName,
            (_, i) => i is null || i.Value == argumentPosition);

    public static ArgumentDescriptor AttributeArgument(Func<IMethodSymbol, bool> attributeConstructorConstraint,
                                                       Func<string, StringComparison, bool> attributeNameConstraint,
                                                       Func<SemanticModel, ILanguageFacade, SyntaxNode, bool> attributeNodeConstraint,
                                                       Func<IParameterSymbol, bool> parameterConstraint,
                                                       Func<IReadOnlyCollection<SyntaxNode>, int?, bool> argumentListConstraint) =>
        new(MemberKind.Attribute,
            invokedMemberConstraint: attributeConstructorConstraint,
            invokedMemberNameConstraint: attributeNameConstraint,
            invokedMemberNodeConstraint: attributeNodeConstraint,
            argumentListConstraint: argumentListConstraint,
            parameterConstraint: parameterConstraint,
            refKind: null);

    public static ArgumentDescriptor AttributeProperty(KnownType attributeType, string propertyName) =>
        AttributeArgument(
            attributeConstructorConstraint: x => x is { MethodKind: MethodKind.PropertySet, AssociatedSymbol: { Name: { } name, ContainingType: { } type } }
                && name == propertyName
                && attributeType.Matches(type),
            attributeNameConstraint: (s, c) => AttributeClassNameConstraint(attributeType.TypeName, s, c),
            (_, _, _) => true,
            parameterConstraint: _ => true,
            argumentListConstraint: (_, _) => true);

    public static ArgumentDescriptor AttributeProperty(string attributeName, string propertyName) =>
        AttributeArgument(
            attributeConstructorConstraint: x => x is { MethodKind: MethodKind.PropertySet, AssociatedSymbol.Name: { } name } && name == propertyName,
            attributeNameConstraint: (s, c) => AttributeClassNameConstraint(attributeName, s, c),
            (_, _, _) => true,
            parameterConstraint: _ => true,
            argumentListConstraint: (_, _) => true);

    private static bool AttributeClassNameConstraint(string attributeTypeName, string nodeClassName, StringComparison c) =>
        nodeClassName.Equals(attributeTypeName, c) || attributeTypeName.Equals($"{nodeClassName}Attribute", c);

    private static ArgumentDescriptor MethodInvocation(KnownType invokedType,
                                                       string methodName,
                                                       Func<IParameterSymbol, bool> parameterConstraint,
                                                       Func<int, bool> argumentPosition,
                                                       RefKind? refKind) =>
        MethodInvocation(invokedType, (n, c) => n.Equals(methodName, c), parameterConstraint, argumentPosition, refKind);
}
