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
    public sealed class DoNotHideBaseClassMethods : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4019";
        private const string MessageFormat = "Remove or rename that method because it hides '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var declarationSyntax = (TypeDeclarationSyntax)c.Node;
                    if (declarationSyntax.Identifier.IsMissing
                        || c.IsRedundantPositionalRecordContext()
                        || !(c.SemanticModel.GetDeclaredSymbol(declarationSyntax) is {} declaredSymbol))
                    {
                        return;
                    }

                    var issueFinder = new IssueFinder(declaredSymbol, c.SemanticModel);
                    foreach (var diagnostic in declarationSyntax.Members.Select(issueFinder.FindIssue).WhereNotNull())
                    {
                        c.ReportIssue(diagnostic);
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKindEx.RecordClassDeclaration);

        private sealed class IssueFinder
        {
            private enum Match
            {
                Different,
                Identical,
                WeaklyDerived
            }

            private readonly IList<IMethodSymbol> allBaseTypeMethods;
            private readonly SemanticModel semanticModel;

            public IssueFinder(ITypeSymbol typeSymbol, SemanticModel semanticModel)
            {
                this.semanticModel = semanticModel;
                allBaseTypeMethods = GetAllBaseMethods(typeSymbol);
            }

            public Diagnostic FindIssue(MemberDeclarationSyntax memberDeclaration)
            {
                var issueLocation = (memberDeclaration as MethodDeclarationSyntax)?.Identifier.GetLocation();

                if (semanticModel.GetDeclaredSymbol(memberDeclaration) is not IMethodSymbol methodSymbol || issueLocation == null)
                {
                    return null;
                }

                var baseMethodHidden = FindBaseMethodHiddenByMethod(methodSymbol);
                return baseMethodHidden != null ? CreateDiagnostic(Rule, issueLocation, baseMethodHidden) : null;
            }

            private static List<IMethodSymbol> GetAllBaseMethods(ITypeSymbol typeSymbol) =>
                typeSymbol.BaseType
                    .GetSelfAndBaseTypes()
                    .SelectMany(t => t.GetMembers())
                    .OfType<IMethodSymbol>()
                    .Where(m => IsSymbolVisibleFromNamespace(m, typeSymbol.ContainingNamespace))
                    .Where(m => m.Parameters.Length > 0)
                    .Where(m => !string.IsNullOrEmpty(m.Name))
                    .ToList();

            private static bool IsSymbolVisibleFromNamespace(ISymbol symbol, INamespaceSymbol ns) =>
                symbol.DeclaredAccessibility != Accessibility.Private
                && (symbol.DeclaredAccessibility != Accessibility.Internal || ns.Equals(symbol.ContainingNamespace));

            private IMethodSymbol FindBaseMethodHiddenByMethod(IMethodSymbol methodSymbol)
            {
                var baseMemberCandidates = allBaseTypeMethods.Where(m => m.Name == methodSymbol.Name);

                IMethodSymbol hiddenBaseMethodCandidate = null;

                var hasBaseMethodWithSameSignature = baseMemberCandidates.Any(baseMember =>
                    {
                        var result = ComputeSignatureMatch(baseMember, methodSymbol);
                        if (result == Match.WeaklyDerived)
                        {
                            hiddenBaseMethodCandidate ??= baseMember;
                        }

                        return result == Match.Identical;
                    });

                return hasBaseMethodWithSameSignature ? null : hiddenBaseMethodCandidate;
            }

            private static Match ComputeSignatureMatch(IMethodSymbol baseMethodSymbol, IMethodSymbol methodSymbol)
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

            private static Match ComputeParameterMatch(IParameterSymbol baseParam, IParameterSymbol methodParam)
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
