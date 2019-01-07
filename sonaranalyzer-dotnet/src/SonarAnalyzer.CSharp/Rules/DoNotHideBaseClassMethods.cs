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
    [Rule(DiagnosticId)]
    public sealed class DoNotHideBaseClassMethods : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4019";
        private const string MessageFormat = "Remove or rename that method because it hides '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;
                    var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);

                    if (classSymbol == null || classDeclaration.Identifier.IsMissing)
                    {
                        return;
                    }

                    var issueFinder = new IssueFinder(classSymbol, c.SemanticModel);

                    classDeclaration
                        .Members
                        .Select(issueFinder.FindIssue)
                        .WhereNotNull()
                        .ToList()
                        .ForEach(d => c.ReportDiagnosticWhenActive(d));
                },
                SyntaxKind.ClassDeclaration);
        }

        private class IssueFinder
        {
            private enum Match { Different, Identical, WeaklyDerived }

            private readonly IList<IMethodSymbol> allBaseClassMethods;
            private readonly SemanticModel semanticModel;

            public IssueFinder(INamedTypeSymbol classSymbol, SemanticModel semanticModel)
            {
                this.semanticModel = semanticModel;
                this.allBaseClassMethods = GetAllBaseMethods(classSymbol);
            }

            private static List<IMethodSymbol> GetAllBaseMethods(INamedTypeSymbol classSymbol)
            {
                return classSymbol.BaseType
                         .GetSelfAndBaseTypes()
                         .SelectMany(t => t.GetMembers())
                         .OfType<IMethodSymbol>()
                         .Where(m => IsSymbolVisibleFromNamespace(m, classSymbol.ContainingNamespace))
                         .Where(m => m.Parameters.Length > 0)
                         .Where(m => !string.IsNullOrEmpty(m.Name))
                         .ToList();
            }

            private static bool IsSymbolVisibleFromNamespace(ISymbol symbol, INamespaceSymbol ns)
            {
                return symbol.DeclaredAccessibility != Accessibility.Private &&
                       (symbol.DeclaredAccessibility != Accessibility.Internal || ns.Equals(symbol.ContainingNamespace));
            }

            public Diagnostic FindIssue(MemberDeclarationSyntax memberDeclaration)
            {
                var issueLocation = (memberDeclaration as MethodDeclarationSyntax)?.Identifier.GetLocation();

                if (!(this.semanticModel.GetDeclaredSymbol(memberDeclaration) is IMethodSymbol methodSymbol) || issueLocation == null)
                {
                    return null;
                }

                var baseMethodHidden = FindBaseMethodHiddenByMethod(methodSymbol);
                return baseMethodHidden != null ? Diagnostic.Create(rule, issueLocation, baseMethodHidden) : null;
            }

            private IMethodSymbol FindBaseMethodHiddenByMethod(IMethodSymbol methodSymbol)
            {
                var baseMemberCandidates = this.allBaseClassMethods.Where(m => m.Name == methodSymbol.Name);

                IMethodSymbol hiddenBaseMethodCandidate = null;

                var hasBaseMethodWithSameSignature = baseMemberCandidates.Any(baseMember =>
                    {
                        var result = ComputeSignatureMatch(baseMember, methodSymbol);
                        if (result == Match.WeaklyDerived)
                        {
                            hiddenBaseMethodCandidate = hiddenBaseMethodCandidate ?? baseMember;
                        }

                        return result == Match.Identical;
                    });

                return hasBaseMethodWithSameSignature ? null : hiddenBaseMethodCandidate;
            }

            private Match ComputeSignatureMatch(IMethodSymbol baseMethodSymbol, IMethodSymbol methodSymbol)
            {
                var baseMethodParams = baseMethodSymbol.Parameters;
                var methodParams = methodSymbol.Parameters;

                if (baseMethodParams.Length != methodParams.Length)
                {
                    return Match.Different;
                }

                var signatureMatch = Match.Identical;
                for (var i = 0; i < methodParams.Length; i++)
                {
                    var match = ComputeParameterMatch(baseMethodParams[i], methodParams[i]);

                    if (match == Match.Different)
                    {
                        return Match.Different;
                    }

                    if (match == Match.WeaklyDerived)
                    {
                        signatureMatch = Match.WeaklyDerived;
                    }
                }

                return signatureMatch;
            }

            private Match ComputeParameterMatch(IParameterSymbol baseParam, IParameterSymbol methodParam)
            {
                if (baseParam.Type.Is(TypeKind.TypeParameter))
                {
                    return methodParam.Type.TypeKind == TypeKind.TypeParameter ? Match.Identical : Match.Different;
                }

                if (Equals(baseParam.Type.OriginalDefinition, methodParam.Type.OriginalDefinition))
                {
                    return Match.Identical;
                }

                return baseParam.Type.DerivesOrImplements(methodParam.Type) ? Match.WeaklyDerived : Match.Different;
            }
        }
    }
}
