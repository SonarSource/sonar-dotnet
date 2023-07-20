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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ImplementIDisposableCorrectly : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3881";
        private const string MessageFormat = "Fix this implementation of 'IDisposable' to conform to the dispose pattern.";

        private static readonly ISet<SyntaxKind> NotAllowedDisposeModifiers = new HashSet<SyntaxKind>
        {
            SyntaxKind.VirtualKeyword,
            SyntaxKind.AbstractKeyword
        };

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    if (c.IsRedundantPositionalRecordContext())
                    {
                        return;
                    }

                    var typeDeclarationSyntax = (TypeDeclarationSyntax)c.Node;
                    var declarationIdentifier = typeDeclarationSyntax.Identifier;
                    var checker = new DisposableChecker(typeDeclarationSyntax.BaseList,
                                                        declarationIdentifier,
                                                        c.SemanticModel.GetDeclaredSymbol(typeDeclarationSyntax),
                                                        c.Node.GetDeclarationTypeName(),
                                                        c.SemanticModel);

                    var locations = checker.GetIssueLocations(typeDeclarationSyntax);
                    if (locations.Any())
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, declarationIdentifier.GetLocation(),
                                                                       locations.ToAdditionalLocations(),
                                                                       locations.ToProperties()));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKindEx.RecordClassDeclaration);

        private sealed class DisposableChecker
        {
            private readonly SemanticModel semanticModel;
            private readonly List<SecondaryLocation> secondaryLocations = new List<SecondaryLocation>();
            private readonly BaseListSyntax baseTypes;
            private readonly SyntaxToken typeIdentifier;
            private readonly INamedTypeSymbol typeSymbol;
            private readonly string nodeType;

            public DisposableChecker(BaseListSyntax baseTypes, SyntaxToken typeIdentifier, INamedTypeSymbol typeSymbol, string nodeType, SemanticModel semanticModel)
            {
                this.baseTypes = baseTypes;
                this.typeIdentifier = typeIdentifier;
                this.typeSymbol = typeSymbol;
                this.nodeType = nodeType;
                this.semanticModel = semanticModel;
            }

            public List<SecondaryLocation> GetIssueLocations(TypeDeclarationSyntax typeDeclarationSyntax)
            {
                if (typeSymbol == null || typeSymbol.IsSealed)
                {
                    return new List<SecondaryLocation>();
                }

                if (typeSymbol.BaseType.Implements(KnownType.System_IDisposable))
                {
                    var iDisposableInterfaceSyntax = baseTypes?.Types.FirstOrDefault(IsOrImplementsIDisposable);
                    if (iDisposableInterfaceSyntax != null)
                    {
                        AddSecondaryLocation(iDisposableInterfaceSyntax.GetLocation(),
                                             $"Remove 'IDisposable' from the list of interfaces implemented by '{typeSymbol.Name}'"
                                             + $" and override the base {nodeType} 'Dispose' implementation instead.");
                    }

                    if (HasVirtualDisposeBool(typeSymbol.BaseType))
                    {
                        VerifyDisposeOverrideCallsBase(FindMethodImplementationOrAbstractDeclaration(typeSymbol, IsDisposeBool, typeDeclarationSyntax)
                                                       .OfType<MethodDeclarationSyntax>()
                                                       .FirstOrDefault());
                    }

                    return secondaryLocations;
                }

                if (typeSymbol.Implements(KnownType.System_IDisposable))
                {
                    if (!FindMethodDeclarations(typeSymbol, IsDisposeBool).Any())
                    {
                        AddSecondaryLocation(typeIdentifier.GetLocation(),
                                             $"Provide 'protected' overridable implementation of 'Dispose(bool)' on "
                                             + $"'{typeSymbol.Name}' or mark the type as 'sealed'.");
                    }

                    var destructor = FindMethodImplementationOrAbstractDeclaration(typeSymbol, SymbolHelper.IsDestructor, typeDeclarationSyntax)
                                     .OfType<DestructorDeclarationSyntax>()
                                     .FirstOrDefault();

                    VerifyDestructor(destructor);

                    var disposeMethod = FindMethodImplementationOrAbstractDeclaration(typeSymbol, KnownMethods.IsIDisposableDispose, typeDeclarationSyntax)
                                        .OfType<MethodDeclarationSyntax>()
                                        .FirstOrDefault();

                    VerifyDispose(disposeMethod, typeSymbol.IsSealed);
                }

                return secondaryLocations;
            }

            private void AddSecondaryLocation(Location location, string message) =>
                secondaryLocations.Add(new SecondaryLocation(location, message));

            private void VerifyDestructor(DestructorDeclarationSyntax destructorSyntax)
            {
                if (!destructorSyntax.HasBodyOrExpressionBody())
                {
                    return;
                }

                if (!HasStatementsCount(destructorSyntax, 1) || !CallsVirtualDispose(destructorSyntax, argumentValue: a => IsLiteralArgument(a, SyntaxKind.FalseKeyword)))
                {
                    AddSecondaryLocation(destructorSyntax.Identifier.GetLocation(),
                                         $"Modify '{typeSymbol.Name}.~{typeSymbol.Name}()' so that it calls 'Dispose(false)' and "
                                         + "then returns.");
                }
            }

            private void VerifyDisposeOverrideCallsBase(MethodDeclarationSyntax disposeMethod)
            {
                if (!disposeMethod.HasBodyOrExpressionBody())
                {
                    return;
                }

                var parameterName = disposeMethod.ParameterList.Parameters.Single().Identifier.Text;

                if (!CallsVirtualDispose(disposeMethod, argumentValue: a => a is { Expression: IdentifierNameSyntax { Identifier.Text: { } text } } && text == parameterName))
                {
                    AddSecondaryLocation(disposeMethod.Identifier.GetLocation(), $"Modify 'Dispose({parameterName})' so that it calls 'base.Dispose({parameterName})'.");
                }
            }

            private void VerifyDispose(MethodDeclarationSyntax disposeMethod, bool isSealedClass)
            {
                if (disposeMethod == null)
                {
                    return;
                }

                if (disposeMethod.HasBodyOrExpressionBody() && !isSealedClass)
                {
                    var missingVirtualDispose = !CallsVirtualDispose(disposeMethod, argumentValue: a => IsLiteralArgument(a, SyntaxKind.TrueKeyword));
                    var missingSuppressFinalize = !CallsSuppressFinalize(disposeMethod);
                    string remediation = null;

                    if (missingVirtualDispose && missingSuppressFinalize)
                    {
                        remediation = "should call 'Dispose(true)' and 'GC.SuppressFinalize(this)'.";
                    }
                    else if (missingVirtualDispose)
                    {
                        remediation = "should also call 'Dispose(true)'.";
                    }
                    else if (missingSuppressFinalize)
                    {
                        remediation = "should also call 'GC.SuppressFinalize(this)'.";
                    }
                    else if (!HasStatementsCount(disposeMethod, 2))
                    {
                        remediation = "should call 'Dispose(true)', 'GC.SuppressFinalize(this)' and nothing else.";
                    }

                    if (remediation != null)
                    {
                        AddSecondaryLocation(disposeMethod.Identifier.GetLocation(), $"'{typeSymbol.Name}.Dispose()' {remediation}");
                    }
                }

                // Because of partial classes we cannot always rely on the current semantic model.
                // See issue: https://github.com/SonarSource/sonar-dotnet/issues/690
                var disposeMethodSymbol = disposeMethod.SyntaxTree.GetSemanticModelOrDefault(semanticModel)?.GetDeclaredSymbol(disposeMethod);
                if (disposeMethodSymbol == null)
                {
                    return;
                }

                if (disposeMethodSymbol.IsAbstract || disposeMethodSymbol.IsVirtual)
                {
                    var modifier = disposeMethod.Modifiers
                                                .FirstOrDefault(m => m.IsAnyKind(NotAllowedDisposeModifiers));

                    AddSecondaryLocation(modifier.GetLocation(), $"'{typeSymbol.Name}.Dispose()' should not be 'virtual' or 'abstract'.");
                }

                if (disposeMethodSymbol.ExplicitInterfaceImplementations.Any())
                {
                    AddSecondaryLocation(disposeMethod.Identifier.GetLocation(), $"'{typeSymbol.Name}.Dispose()' should be 'public'.");
                }
            }

            private bool IsOrImplementsIDisposable(BaseTypeSyntax baseType) =>
                (semanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol).Is(KnownType.System_IDisposable);

            private bool CallsSuppressFinalize(BaseMethodDeclarationSyntax methodDeclaration) =>
                methodDeclaration.ContainsMethodInvocation(semanticModel,
                    method => method.Expression.NameIs(nameof(GC.SuppressFinalize))
                        && method is { ArgumentList.Arguments: { Count: 1 } arguments }
                        && arguments[0] is { Expression: ThisExpressionSyntax },
                    KnownMethods.IsGcSuppressFinalize);

            private bool CallsVirtualDispose(BaseMethodDeclarationSyntax methodDeclaration, Func<ArgumentSyntax, bool> argumentValue) =>
                methodDeclaration.ContainsMethodInvocation(semanticModel,
                    method => method.Expression.NameIs(nameof(IDisposable.Dispose))
                        && method is { ArgumentList.Arguments: { Count: 1 } arguments }
                        && arguments[0] is var argument
                        && argumentValue(argument),
                    IsDisposeBool);

            private static bool IsDisposeBool(IMethodSymbol method) =>
                method.Name == nameof(IDisposable.Dispose)
                && (method.IsVirtual || method.IsAbstract || method.IsOverride)
                && method.DeclaredAccessibility == Accessibility.Protected
                && method.Parameters.Length == 1
                && method.Parameters.Any(p => p.Type.Is(KnownType.System_Boolean));

            private static bool HasStatementsCount(BaseMethodDeclarationSyntax methodDeclaration, int expectedStatementsCount) =>
                methodDeclaration.Body?.Statements.Count == expectedStatementsCount
                || (methodDeclaration.ExpressionBody() != null && expectedStatementsCount == 1); // Expression body has only one statement

            private static IEnumerable<SyntaxNode> FindMethodDeclarations(INamedTypeSymbol typeSymbol, Func<IMethodSymbol, bool> predicate) =>
                typeSymbol.GetMembers().OfType<IMethodSymbol>().Where(predicate).Select(x => x.ImplementationSyntax());

            private static IEnumerable<SyntaxNode> FindMethodImplementationOrAbstractDeclaration(INamedTypeSymbol typeSymbol,
                                                                                                 Func<IMethodSymbol, bool> predicate,
                                                                                                 TypeDeclarationSyntax typeDeclarationSyntax) =>
                FindMethodDeclarations(typeSymbol, predicate)
                    .OfType<BaseMethodDeclarationSyntax>()
                    // We want to skip the partial method declarations when reporting secondary issues since the messages are relevant only for implementation part.
                    // We do want to include abstract methods though since the implementation is in another type which could be defined in a different assembly than the one analyzed.
                    .Where(x => typeDeclarationSyntax.Contains(x) && (x.HasBodyOrExpressionBody() || x.Modifiers.AnyOfKind(SyntaxKind.AbstractKeyword)));

            private static bool HasVirtualDisposeBool(ITypeSymbol typeSymbol) =>
                typeSymbol.GetSelfAndBaseTypes()
                          .SelectMany(type => type.GetMembers())
                          .OfType<IMethodSymbol>()
                          .Where(IsDisposeBool)
                          .Any(symbol => !symbol.IsAbstract);

            private static bool IsLiteralArgument(ArgumentSyntax argument, SyntaxKind literalTokenKind) =>
                argument is { Expression: LiteralExpressionSyntax { Token: var token } } && token.IsKind(literalTokenKind);
        }
    }
}
