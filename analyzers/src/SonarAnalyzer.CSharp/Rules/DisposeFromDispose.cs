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
    public sealed class DisposeFromDispose : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2952";
        private const string MessageFormat = "Move this 'Dispose' call into this class' own 'Dispose' method.";

        private const string DisposeMethodName = nameof(IDisposable.Dispose);
        private const string DisposeMethodExplicitName = "System.IDisposable.Dispose";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                var invocation = (InvocationExpressionSyntax)c.Node;
                var languageVersion = c.Compilation.GetLanguageVersion();
                if (InvocationTargetAndName(invocation, out var fieldCandidate, out var name)
                    && c.SemanticModel.GetSymbolInfo(fieldCandidate).Symbol is IFieldSymbol invocationTarget
                    && invocationTarget.IsNonStaticNonPublicDisposableField(languageVersion)
                    && IsDisposeMethodCalled(invocation, c.SemanticModel, languageVersion)
                    && IsDisposableClassOrStruct(invocationTarget.ContainingType, languageVersion)
                    && !IsCalledInsideDispose(invocation, c.SemanticModel)
                    && FieldDeclaredInType(c.SemanticModel, invocation, invocationTarget)
                    && !FieldDisposedInDispose(c.SemanticModel, invocationTarget))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, name.GetLocation()));
                }
            },
            SyntaxKind.InvocationExpression);

        private static bool FieldDisposedInDispose(SemanticModel model, IFieldSymbol invocationTarget) =>
            invocationTarget.ContainingSymbol is ITypeSymbol container
            && container.FindImplementationForInterfaceMember(IDisposableDisposeMethodSymbol(model.Compilation)) is IMethodSymbol dispose
            && FieldIsDisposedIn(model, invocationTarget, dispose);

        private static bool FieldIsDisposedIn(SemanticModel model, IFieldSymbol invocationTarget, IMethodSymbol dispose) =>
            (dispose.PartialImplementationPart ?? dispose).DeclaringSyntaxReferences
            .SelectMany(x => x.GetSyntax()
                .DescendantNodesAndSelf(x => !x.IsAnyKind(
                    SyntaxKindEx.LocalFunctionStatement,
                    SyntaxKind.ParenthesizedLambdaExpression,
                    SyntaxKind.SimpleLambdaExpression,
                    SyntaxKind.AnonymousMethodExpression))
            .OfType<InvocationExpressionSyntax>())
            .Any(x => InvocationTargetAndName(x, out var target, out var name)
                && name.NameIs(DisposeMethodName)
                && x.EnsureCorrectSemanticModelOrDefault(model) is { } correctModel
                && correctModel.GetSymbolInfo(target).Symbol is IFieldSymbol field
                && field.Equals(invocationTarget)
                && correctModel.GetSymbolInfo(name).Symbol is IMethodSymbol invokedDispose
                && invokedDispose.Equals(field.Type.FindImplementationForInterfaceMember(IDisposableDisposeMethodSymbol(correctModel.Compilation))));

        private static bool FieldDeclaredInType(SemanticModel model, InvocationExpressionSyntax invocation, IFieldSymbol invocationTarget) =>
            invocation.GetTopMostContainingMethod() is { } containingMethod
            && containingMethod.EnsureCorrectSemanticModelOrDefault(model) is { } correctModel
            && (correctModel.GetDeclaredSymbol(containingMethod)?.ContainingSymbol?.Equals(invocationTarget.ContainingType) ?? true);

        private static bool InvocationTargetAndName(InvocationExpressionSyntax invocation, out ExpressionSyntax target, out SimpleNameSyntax name)
        {
            switch (invocation.Expression)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    name = memberAccess.Name;
                    target = memberAccess.Expression;
                    return true;
                case MemberBindingExpressionSyntax memberBinding:
                    name = memberBinding.Name;
                    target = memberBinding.GetParentConditionalAccessExpression().Expression;
                    return true;
                default:
                    target = null;
                    name = null;
                    return false;
            }
        }

        /// <summary>
        /// Classes and structs are disposable if they implement the IDisposable interface.
        /// Starting C# 8, "ref structs" (which cannot implement an interface) can also be disposable.
        /// </summary>
        private static bool IsDisposableClassOrStruct(INamedTypeSymbol type, LanguageVersion languageVersion) =>
            ImplementsDisposable(type) || type.IsDisposableRefStruct(languageVersion);

        private static bool IsCalledInsideDispose(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            semanticModel.GetEnclosingSymbol(invocation.SpanStart) is IMethodSymbol enclosingMethodSymbol
            && IsMethodMatchingDisposeMethodName(enclosingMethodSymbol);

        /// <summary>
        /// Verifies that the invocation is calling the correct Dispose() method on an disposable object.
        /// </summary>
        /// <remarks>
        /// Disposable ref structs do not implement the IDisposable interface and are supported starting C# 8.
        /// </remarks>
        private static bool IsDisposeMethodCalled(InvocationExpressionSyntax invocation, SemanticModel semanticModel, LanguageVersion languageVersion) =>
            semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
            && KnownMethods.IsIDisposableDispose(methodSymbol)
            && IDisposableDisposeMethodSymbol(semanticModel.Compilation) is { } disposeMethodSignature
            && (methodSymbol.Equals(methodSymbol.ContainingType.FindImplementationForInterfaceMember(disposeMethodSignature))
                || methodSymbol.ContainingType.IsDisposableRefStruct(languageVersion));

        private static IMethodSymbol IDisposableDisposeMethodSymbol(Compilation compilation)
            => compilation.SpecialTypeMethod(SpecialType.System_IDisposable, DisposeMethodName);

        private static bool IsMethodMatchingDisposeMethodName(IMethodSymbol enclosingMethodSymbol) =>
            enclosingMethodSymbol.Name == DisposeMethodName
            || (enclosingMethodSymbol.ExplicitInterfaceImplementations.Any() && enclosingMethodSymbol.Name == DisposeMethodExplicitName);

        private static bool ImplementsDisposable(INamedTypeSymbol containingType) =>
            containingType.Implements(KnownType.System_IDisposable);
    }
}
