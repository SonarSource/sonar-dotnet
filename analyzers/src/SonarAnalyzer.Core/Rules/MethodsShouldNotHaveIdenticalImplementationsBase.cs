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

namespace SonarAnalyzer.Rules;

public abstract class MethodsShouldNotHaveIdenticalImplementationsBase<TSyntaxKind, TMethodDeclarationSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S4144";

    protected abstract TSyntaxKind[] SyntaxKinds { get; }

    protected abstract IEnumerable<TMethodDeclarationSyntax> GetMethodDeclarations(SyntaxNode node);
    protected abstract SyntaxToken GetMethodIdentifier(TMethodDeclarationSyntax method);
    protected abstract bool AreDuplicates(SemanticModel model, TMethodDeclarationSyntax firstMethod, TMethodDeclarationSyntax secondMethod);

    protected override string MessageFormat => "Update this method so that its implementation is not identical to '{0}'.";

    protected MethodsShouldNotHaveIdenticalImplementationsBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
            c =>
            {
                if (IsExcludedFromBeingExamined(c))
                {
                    return;
                }

                var methodsToHandle = new LinkedList<TMethodDeclarationSyntax>(GetMethodDeclarations(c.Node));

                while (methodsToHandle.Any())
                {
                    var method = methodsToHandle.First.Value;
                    methodsToHandle.RemoveFirst();
                    var duplicates = methodsToHandle.Where(x => AreDuplicates(c.SemanticModel, method, x)).ToList();

                    foreach (var duplicate in duplicates)
                    {
                        methodsToHandle.Remove(duplicate);
                        var identifier = GetMethodIdentifier(method);
                        var otherIdentifier = GetMethodIdentifier(duplicate);
                        c.ReportIssue(SupportedDiagnostics[0], otherIdentifier, [identifier.ToSecondaryLocation(MessageFormat, otherIdentifier.ValueText)], identifier.ValueText);
                    }
                }
            },
            SyntaxKinds);

    protected virtual bool IsExcludedFromBeingExamined(SonarSyntaxNodeReportingContext context) =>
        context.ContainingSymbol.Kind != SymbolKind.NamedType;

    protected static bool HaveSameParameters<TSyntax>(SeparatedSyntaxList<TSyntax>? leftParameters, SeparatedSyntaxList<TSyntax>? rightParameters)
        where TSyntax : SyntaxNode =>
        (leftParameters is null && rightParameters is null) // In VB.Net the parameter list can be omitted
        || (leftParameters?.Count == rightParameters?.Count && HaveSameParameterLists(leftParameters.Value, rightParameters.Value));

    protected static bool HaveSameTypeParameters<TSyntax>(SemanticModel model, SeparatedSyntaxList<TSyntax>? firstTypeParameterList, SeparatedSyntaxList<TSyntax>? secondTypeParameterList)
        where TSyntax : SyntaxNode
    {
        var firstSymbols = firstTypeParameterList?.Select(x => model.GetDeclaredSymbol(x)).OfType<ITypeParameterSymbol>() ?? [];
        var secondSymbols = secondTypeParameterList?.Select(x => model.GetDeclaredSymbol(x)).OfType<ITypeParameterSymbol>().ToArray() ?? [];
        return firstSymbols.All(x => Array.Exists(secondSymbols, secondSymbol => TypeParametersHaveSameNameAndConstraints(x, secondSymbol)));
    }

    protected static bool AreTheSameType(SemanticModel model, SyntaxNode first, SyntaxNode second) =>
        (first is null && second is null)
        || (first is not null && second is not null
            && model.GetTypeInfo(first).Type?.Equals(model.GetTypeInfo(second).Type) == true);

    private static bool HaveSameParameterLists<TSyntax>(SeparatedSyntaxList<TSyntax> firstParameters,
                                                        SeparatedSyntaxList<TSyntax> secondParameters) where TSyntax : SyntaxNode =>
        firstParameters.Equals(secondParameters, (first, second) => first.IsEquivalentTo(second)); // Perf: Syntactic equivalence for all parameters

    private static bool TypeParametersHaveSameNameAndConstraints(ITypeParameterSymbol first, ITypeParameterSymbol second) =>
        first.Name == second.Name
        && first.HasConstructorConstraint == second.HasConstructorConstraint
        && first.HasReferenceTypeConstraint == second.HasReferenceTypeConstraint
        && first.HasValueTypeConstraint == second.HasValueTypeConstraint
        && first.HasUnmanagedTypeConstraint() == second.HasUnmanagedTypeConstraint()
        && first.ConstraintTypes.Length == second.ConstraintTypes.Length
        && first.ConstraintTypes.All(x => second.ConstraintTypes.Any(y => TypeConstraintsAreSame(x, y)));

    private static bool TypeConstraintsAreSame(ITypeSymbol first, ITypeSymbol second) =>
        first.Equals(second) // M1<T>(T x) where T: IComparable <-> M2<T>(T x) where T: IComparable
        || AreSameNamedTypeParameters(first, second) // M1<T1, T2>() where T1: T2 <-> M2<T1, T2>() where T1: T2
                                                     // T2 of M1 is a different symbol than T2 of M2, but if they have the same name they can be interchanged.
                                                     // T2 equivalency is checked as well by the TypeConstraintsAreSame call in TypeParametersHaveSameNameAndConstraints.
        || TypesAreSameGenericType(first, second); // M1<T>(T x) where T: IEquatable<T> <-> M2<T>(T x) where T: IEquatable<T>

    private static bool TypesAreSameGenericType(ITypeSymbol firstParameterType, ITypeSymbol secondParameterType) =>
        firstParameterType is INamedTypeSymbol { IsGenericType: true } namedTypeFirst
        && secondParameterType is INamedTypeSymbol { IsGenericType: true } namedTypeSecond
        && namedTypeFirst.OriginalDefinition.Equals(namedTypeSecond.OriginalDefinition)
        && namedTypeFirst.TypeArguments.Length == namedTypeSecond.TypeArguments.Length
        && namedTypeFirst.TypeArguments.Equals(namedTypeSecond.TypeArguments, TypeConstraintsAreSame);

    private static bool AreSameNamedTypeParameters(ITypeSymbol first, ITypeSymbol second) =>
        first is ITypeParameterSymbol x && second is ITypeParameterSymbol y && x.Name == y.Name;
}
