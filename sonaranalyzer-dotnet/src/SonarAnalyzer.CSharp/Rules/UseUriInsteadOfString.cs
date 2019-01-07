/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId_RuleS3994)]
    [Rule(DiagnosticId_RuleS3995)]
    [Rule(DiagnosticId_RuleS3996)]
    [Rule(DiagnosticId_RuleS3997)]
    [Rule(DiagnosticId_RuleS4005)]
    public sealed class UseUriInsteadOfString : SonarDiagnosticAnalyzer
    {
        #region Rule definition
        internal const string DiagnosticId_RuleS3994 = "S3994";
        private const string MessageFormat_RuleS3994 = "Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.";
        private static readonly DiagnosticDescriptor rule_S3994 =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId_RuleS3994, MessageFormat_RuleS3994, RspecStrings.ResourceManager);

        internal const string DiagnosticId_RuleS3995 = "S3995";
        private const string MessageFormat_RuleS3995 = "Change this return type to 'System.Uri'.";
        private static readonly DiagnosticDescriptor rule_S3995 =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId_RuleS3995, MessageFormat_RuleS3995, RspecStrings.ResourceManager);

        internal const string DiagnosticId_RuleS3996 = "S3996";
        private const string MessageFormat_RuleS3996 = "Change this property type to 'System.Uri'.";
        private static readonly DiagnosticDescriptor rule_S3996 =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId_RuleS3996, MessageFormat_RuleS3996, RspecStrings.ResourceManager);

        internal const string DiagnosticId_RuleS3997 = "S3997";
        private const string MessageFormat_RuleS3997 = "Refactor this method so it invokes the overload accepting a 'System.Uri' parameter.";
        private static readonly DiagnosticDescriptor rule_S3997 =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId_RuleS3997, MessageFormat_RuleS3997, RspecStrings.ResourceManager);

        internal const string DiagnosticId_RuleS4005 = "S4005";
        private const string MessageFormat_RuleS4005 = "Call the overload that takes a 'System.Uri' as an argument instead.";
        private static readonly DiagnosticDescriptor rule_S4005 =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId_RuleS4005, MessageFormat_RuleS4005, RspecStrings.ResourceManager);
        #endregion

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(rule_S3994, rule_S3995, rule_S3996, rule_S3997, rule_S4005);

        private static readonly HashSet<string> UrlNameVariants = new HashSet<string> { "URI", "URL", "URN" };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(VerifyMethodDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(VerifyPropertyDeclaration,
                SyntaxKind.PropertyDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(VerifyInvocationAndCreation,
                SyntaxKind.InvocationExpression,
                SyntaxKind.ObjectCreationExpression);
        }

        private void VerifyMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (BaseMethodDeclarationSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);

            if (methodSymbol == null || methodSymbol.IsOverride)
            {
                return;
            }

            VerifyReturnType(context, methodDeclaration, methodSymbol);

            var stringUrlParams = GetStringUrlParamIndexes(methodSymbol);
            if (!stringUrlParams.Any())
            {
                return;
            }

            var methodOverloads = FindOverloadsThatUseUriTypeInPlaceOfString(methodSymbol, stringUrlParams);
            if (methodOverloads.Any())
            {
                var methodOverloadSet = new HashSet<IMethodSymbol>(methodOverloads);
                if (!methodDeclaration.IsKind(SyntaxKind.ConstructorDeclaration) &&
                    !methodDeclaration.ContainsMethodInvocation(context.SemanticModel, mSyntax => true,
                        mSymbol => methodOverloadSet.Contains(mSymbol)))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule_S3997, methodDeclaration.FindIdentifierLocation()));
                }
            }
            else
            {
                foreach (var paramIdx in stringUrlParams)
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule_S3994,
                        methodDeclaration.ParameterList.Parameters[paramIdx].Type.GetLocation()));
                }
            }
        }

        private void VerifyPropertyDeclaration(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
            var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);

            if (propertySymbol.Type.Is(KnownType.System_String) &&
                !propertySymbol.IsOverride &&
                NameContainsUri(propertySymbol.Name))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule_S3996, propertyDeclaration.Type.GetLocation()));
            }
        }

        private void VerifyInvocationAndCreation(SyntaxNodeAnalysisContext context)
        {
            if (!(context.SemanticModel.GetSymbolInfo(context.Node).Symbol is IMethodSymbol invokedMethodSymbol) ||
                invokedMethodSymbol.IsInType(KnownType.System_Uri))
            {
                return;
            }

            var stringUrlParams = GetStringUrlParamIndexes(invokedMethodSymbol);
            var methodOverloads = FindOverloadsThatUseUriTypeInPlaceOfString(invokedMethodSymbol, stringUrlParams);
            if (stringUrlParams.Count > 0 && methodOverloads.Any())
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule_S4005, context.Node.GetLocation()));
            }
        }

        private void VerifyReturnType(SyntaxNodeAnalysisContext context,
            BaseMethodDeclarationSyntax methodDeclaration, IMethodSymbol methodSymbol)
        {
            var returnTypeLocation = (methodDeclaration as MethodDeclarationSyntax)?.ReturnType?.GetLocation();

            if (returnTypeLocation != null &&
                methodSymbol.ReturnType.Is(KnownType.System_String) &&
                NameContainsUri(methodSymbol.Name))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule_S3995, returnTypeLocation));
            }
        }

        private IEnumerable<IMethodSymbol> FindOverloadsThatUseUriTypeInPlaceOfString(IMethodSymbol originalMethodSymbol,
            ISet<int> paramIdx)
        {
            if (paramIdx.Count == 0)
            {
                yield break;
            }

            foreach (var methodSymbol in GetOtherMethodOverrides(originalMethodSymbol))
            {
                if (methodSymbol.Parameters.Where((paramSymbol, i)
                        => UsesUriInPlaceOfStringUri(paramSymbol, originalMethodSymbol.Parameters[i], paramIdx.Contains(i)))
                    .Any())
                {
                    yield return methodSymbol;
                }
            }
        }

        private ISet<int> GetStringUrlParamIndexes(IMethodSymbol methodSymbol)
        {
            var set = new HashSet<int>();

            var i = 0;
            foreach (var paramSymbol in methodSymbol.Parameters)
            {
                if (paramSymbol.Type.Is(KnownType.System_String) &&
                    NameContainsUri(paramSymbol.Name))
                {
                    set.Add(i);
                }
                i++;
            }
            return set;
        }

        private IEnumerable<IMethodSymbol> GetOtherMethodOverrides(IMethodSymbol methodSymbol)
        {
            return methodSymbol.ContainingType
                .GetMembers(methodSymbol.Name)
                .OfType<IMethodSymbol>()
                .Where(m => m.Parameters.Length == methodSymbol.Parameters.Length)
                .Where(m => !Equals(m, methodSymbol));
        }

        private bool UsesUriInPlaceOfStringUri(IParameterSymbol paramSymbol, IParameterSymbol originalParamSymbol,
            bool isStringUri)
        {
            return isStringUri ? paramSymbol.Type.Is(KnownType.System_Uri) : Equals(paramSymbol, originalParamSymbol);
        }

        private static bool NameContainsUri(string name)
        {
            var wordsInName = name.SplitCamelCaseToWords();
            return UrlNameVariants.Overlaps(wordsInName);
        }
    }
}
