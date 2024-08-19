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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DisposeNotImplementingDispose : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2953";
        private const string MessageFormat = "Either implement 'IDisposable.Dispose', or totally rename this method to prevent confusion.";
        private const string DisposeMethodName = "Dispose";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                c =>
                {
                    var declaredSymbol = (INamedTypeSymbol)c.Symbol;

                    // ref structs cannot inherit from the IDisposable interface
                    if (declaredSymbol.IsRefStruct())
                    {
                        return;
                    }

                    var disposeMethod = c.Compilation.SpecialTypeMethod(SpecialType.System_IDisposable, "Dispose");
                    if (disposeMethod == null)
                    {
                        return;
                    }

                    var mightImplementDispose = new HashSet<IMethodSymbol>();
                    var namedDispose = new HashSet<IMethodSymbol>();

                    var methods = declaredSymbol.GetMembers(DisposeMethodName).OfType<IMethodSymbol>();
                    foreach (var method in methods)
                    {
                        CollectMethodsNamedAndImplementingDispose(method, disposeMethod, namedDispose, mightImplementDispose);
                    }

                    var disposeMethodsCalledFromDispose = new HashSet<IMethodSymbol>();
                    CollectInvocationsFromDisposeImplementation(disposeMethod, c.Compilation, mightImplementDispose, disposeMethodsCalledFromDispose);

                    ReportDisposeMethods(c, namedDispose.Except(mightImplementDispose).Where(m => !disposeMethodsCalledFromDispose.Contains(m)));
                },
                SymbolKind.NamedType);

        private static void CollectInvocationsFromDisposeImplementation(IMethodSymbol disposeMethod, Compilation compilation,
                                                                        HashSet<IMethodSymbol> mightImplementDispose,
                                                                        HashSet<IMethodSymbol> disposeMethodsCalledFromDispose)
        {
            foreach (var method in mightImplementDispose
                .Where(method => MethodIsDisposeImplementation(method, disposeMethod)))
            {
                var methodDeclarations = method.DeclaringSyntaxReferences
                    .Select(x => new NodeAndModel<MethodDeclarationSyntax>(compilation.GetSemanticModel(x.SyntaxTree), x.GetSyntax() as MethodDeclarationSyntax))
                    .Where(x => x.Node != null);

                var methodDeclaration = methodDeclarations.FirstOrDefault(m => m.Node.HasBodyOrExpressionBody());
                if (methodDeclaration == null)
                {
                    continue;
                }

                var invocations = methodDeclaration.Node.DescendantNodes().OfType<InvocationExpressionSyntax>();
                foreach (var invocation in invocations)
                {
                    CollectDisposeMethodsCalledFromDispose(invocation, methodDeclaration.Model, disposeMethodsCalledFromDispose);
                }
            }
        }

        private static void CollectDisposeMethodsCalledFromDispose(InvocationExpressionSyntax invocationExpression,
                                                                   SemanticModel semanticModel,
                                                                   HashSet<IMethodSymbol> disposeMethodsCalledFromDispose)
        {
            if (!invocationExpression.IsOnThis())
            {
                return;
            }

            if (!(semanticModel.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol invokedMethod) || invokedMethod.Name != DisposeMethodName)
            {
                return;
            }

            disposeMethodsCalledFromDispose.Add(invokedMethod);
        }

        private static void ReportDisposeMethods(SonarSymbolReportingContext context, IEnumerable<IMethodSymbol> disposeMethods)
        {
            foreach (var disposeMethod in disposeMethods)
            {
                foreach (var location in disposeMethod.Locations)
                {
                    context.ReportIssue(Rule, location, disposeMethod.PartialImplementationPart?.Locations.ToSecondary() ?? []);
                }
            }
        }

        private static void CollectMethodsNamedAndImplementingDispose(IMethodSymbol methodSymbol,
                                                                      IMethodSymbol disposeMethod,
                                                                      HashSet<IMethodSymbol> namedDispose,
                                                                      HashSet<IMethodSymbol> mightImplementDispose)
        {
            if (methodSymbol.Name != DisposeMethodName)
            {
                return;
            }

            namedDispose.Add(methodSymbol);

            if (methodSymbol.IsOverride
                || MethodIsDisposeImplementation(methodSymbol, disposeMethod)
                || MethodMightImplementDispose(methodSymbol))
            {
                mightImplementDispose.Add(methodSymbol);
            }
        }

        private static bool MethodIsDisposeImplementation(IMethodSymbol methodSymbol, IMethodSymbol disposeMethod) =>
            methodSymbol.Equals(methodSymbol.ContainingType.FindImplementationForInterfaceMember(disposeMethod));

        private static bool MethodMightImplementDispose(IMethodSymbol declaredMethodSymbol)
        {
            var containingType = declaredMethodSymbol.ContainingType;

            if (containingType.BaseType is { Kind: SymbolKind.ErrorType })
            {
                return true;
            }

            var interfaces = containingType.AllInterfaces;
            foreach (var @interface in interfaces)
            {
                if (@interface.Kind == SymbolKind.ErrorType)
                {
                    return true;
                }

                var interfaceMethods = @interface.GetMembers().OfType<IMethodSymbol>();
                if (interfaceMethods.Any(interfaceMethod => declaredMethodSymbol.Equals(containingType.FindImplementationForInterfaceMember(interfaceMethod))))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
