/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
                        c.ReportIssue(SupportedDiagnostics[0], GetMethodIdentifier(duplicate), [GetMethodIdentifier(method).ToSecondaryLocation()], GetMethodIdentifier(method).ValueText);
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
        return firstSymbols.All(x => Array.Exists(secondSymbols, secondSymbol => TypesAreEquivalentTypeParameters(x, secondSymbol)));
    }

    private static bool HaveSameParameterLists<TSyntax>(SeparatedSyntaxList<TSyntax> leftParameters,
                                                        SeparatedSyntaxList<TSyntax> rightParameters) where TSyntax : SyntaxNode =>
        leftParameters.Equals(rightParameters, (left, right) => left.IsEquivalentTo(right)); // Perf: Syntactic equivalence for all parameters

    private static bool TypesAreSame(ITypeSymbol leftParameterType, ITypeSymbol rightParameterType) =>
        leftParameterType.Equals(rightParameterType) // M1(int x) <-> M2(int x)
        || TypesAreEquivalentTypeParameters(leftParameterType, rightParameterType) // M1<T>(T x) where T: class <-> M2<T>(T x) where T: class
                                                                                   // T of M1 is a different symbol than T of M2, but if they have the same constraints, they can be interchanged
        || TypesAreSameGenericType(leftParameterType, rightParameterType); // M1<T>(IEnumerable<T> x) where T: class <-> M2<T>(IEnumerable<T> x) where T: class

    private static bool TypesAreSameGenericType(ITypeSymbol leftParameterType, ITypeSymbol rightParameterType) =>
        leftParameterType is INamedTypeSymbol { IsGenericType: true } namedTypeLeft
        && rightParameterType is INamedTypeSymbol { IsGenericType: true } namedTypeRight
        && namedTypeLeft.TypeArguments.Length == namedTypeRight.TypeArguments.Length
        && namedTypeLeft.TypeArguments.Equals(namedTypeRight.TypeArguments, (x, y) =>
            x is ITypeParameterSymbol a
                ? y is ITypeParameterSymbol b && a.Name == b.Name
                : TypesAreSame(x, y));

    private static bool TypesAreEquivalentTypeParameters(ITypeSymbol leftParameterType, ITypeSymbol rightParameterType) =>
        leftParameterType.Equals(rightParameterType) || TypeParametersHaveSameNameAndConstraints(leftParameterType, rightParameterType);

    private static bool TypeParametersHaveSameNameAndConstraints(ITypeSymbol leftParameterType, ITypeSymbol rightParameterType) =>
        leftParameterType is ITypeParameterSymbol left
        && rightParameterType is ITypeParameterSymbol right
        && left.Name == right.Name
        && left.HasConstructorConstraint == right.HasConstructorConstraint
        && left.HasReferenceTypeConstraint == right.HasReferenceTypeConstraint
        && left.HasValueTypeConstraint == right.HasValueTypeConstraint
        && left.HasUnmanagedTypeConstraint() == right.HasUnmanagedTypeConstraint()
        && left.ConstraintTypes.Length == right.ConstraintTypes.Length
        && left.ConstraintTypes.All(x => right.ConstraintTypes.Any(y => TypesAreSame(x, y)));
}
