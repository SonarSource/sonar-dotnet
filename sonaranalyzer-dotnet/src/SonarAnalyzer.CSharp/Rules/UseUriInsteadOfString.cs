/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId_RuleS3994)]
    [Rule(DiagnosticId_RuleS3995)]
    [Rule(DiagnosticId_RuleS3996)]
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

        internal const string DiagnosticId_RuleS4005 = "S4005";
        private const string MessageFormat_RuleS4005 = "Call the overload that takes a 'System.Uri' as an argument instead.";
        private static readonly DiagnosticDescriptor rule_S4005 =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId_RuleS4005, MessageFormat_RuleS4005, RspecStrings.ResourceManager);
        #endregion

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(rule_S3994, rule_S3995, rule_S3996, rule_S4005);

        private static readonly HashSet<string> UrlNameVariants = new HashSet<string> { "uri", "url", "urn" };

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodDeclaration);

                    if (methodSymbol.IsOverride)
                    {
                        return;
                    }

                    if (methodSymbol.ReturnType.Is(KnownType.System_String) &&
                        NameContainsUri(methodSymbol.Name))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule_S3995, methodDeclaration.ReturnType.GetLocation()));
                    }

                    var stringUrlParams = GetStringUrlParamIndexes(methodSymbol);
                    if (!stringUrlParams.Any() ||
                        HasOverloadThatUsesUriTypeInPlaceOfString(methodSymbol, stringUrlParams))
                    {
                        return;
                    }

                    foreach (int paramIdx in stringUrlParams)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule_S3994,
                            methodDeclaration.ParameterList.Parameters[paramIdx].Type.GetLocation()));
                    }
                },
                SyntaxKind.MethodDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var propertyDeclaration = (PropertyDeclarationSyntax)c.Node;
                    var propertySymbol = c.SemanticModel.GetDeclaredSymbol(propertyDeclaration);

                    if (propertySymbol.Type.Is(KnownType.System_String) &&
                        !propertySymbol.IsOverride &&
                        NameContainsUri(propertySymbol.Name))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule_S3996, propertyDeclaration.Type.GetLocation()));
                    }
                },
                SyntaxKind.PropertyDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var invokedMethod = c.SemanticModel.GetSymbolInfo(c.Node).Symbol as IMethodSymbol;
                    if (invokedMethod == null)
                    {
                        return;
                    }

                    if (invokedMethod.IsInType(KnownType.System_Uri))
                    {
                        return;
                    }

                    var stringUrlParams = GetStringUrlParamIndexes(invokedMethod);
                    if (!stringUrlParams.Any())
                    {
                        return;
                    }

                    if (HasOverloadThatUsesUriTypeInPlaceOfString(invokedMethod, stringUrlParams))
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule_S4005, c.Node.GetLocation()));
                    }
                },
                SyntaxKind.InvocationExpression,
                SyntaxKind.ObjectCreationExpression);
        }

        private bool HasOverloadThatUsesUriTypeInPlaceOfString(IMethodSymbol originalMethodSymbol, ISet<int> paramIdx)
        {
            foreach (var methodSymbol in GetOtherMethodOverrides(originalMethodSymbol))
            {
                if (methodSymbol.Parameters
                    .Where((paramSymbol, i) => UsesUriInPlaceOfStringUri(paramSymbol, originalMethodSymbol.Parameters[i], paramIdx.Contains(i)))
                    .Any())
                {
                    return true;
                }
            }
            return false;
        }

        private ISet<int> GetStringUrlParamIndexes(IMethodSymbol methodSymbol)
        {
            var set = new HashSet<int>();

            int i = 0;
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
                .Where(m => m.Parameters.Count() == methodSymbol.Parameters.Count())
                .Where(m => !Equals(m, methodSymbol));
        }

        private bool UsesUriInPlaceOfStringUri(IParameterSymbol paramSymbol, IParameterSymbol originalParamSymbol, bool isStringUri)
        {
            return isStringUri ? paramSymbol.Type.Is(KnownType.System_Uri) :
                Equals(paramSymbol, originalParamSymbol);
        }

        private static bool NameContainsUri(string name)
        {
            var wordsInName = name.SplitCamelCaseToWords();
            return UrlNameVariants.Overlaps(wordsInName);
        }
    }
}
