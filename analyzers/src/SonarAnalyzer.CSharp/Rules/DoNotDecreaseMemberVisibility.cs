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
    public sealed class DoNotDecreaseMemberVisibility : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4015";
        private const string MessageFormat = "This member hides '{0}'. Make it non-private or seal the class.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var classDeclaration = (TypeDeclarationSyntax)c.Node;
                    if (classDeclaration.Identifier.IsMissing
                        || c.IsRedundantPositionalRecordContext()
                        || !(c.ContainingSymbol is ITypeSymbol { IsSealed: false } classSymbol))
                    {
                        return;
                    }

                    var issueReporter = new IssueReporter(classSymbol, c);
                    foreach (var member in classDeclaration.Members)
                    {
                        issueReporter.ReportIssue(member);
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKindEx.RecordClassDeclaration);

        private sealed class IssueReporter
        {
            private readonly IList<IMethodSymbol> allBaseClassMethods;
            private readonly IList<IPropertySymbol> allBaseClassProperties;
            private readonly SonarSyntaxNodeReportingContext context;

            public IssueReporter(ITypeSymbol classSymbol, SonarSyntaxNodeReportingContext context)
            {
                this.context = context;
                var allBaseClassMembers = classSymbol.BaseType
                        .GetSelfAndBaseTypes()
                        .SelectMany(t => t.GetMembers())
                        .Where(symbol => IsSymbolVisibleFromNamespace(symbol, classSymbol.ContainingNamespace))
                        .ToList();

                allBaseClassMethods = allBaseClassMembers.OfType<IMethodSymbol>().ToList();
                allBaseClassProperties = allBaseClassMembers.OfType<IPropertySymbol>().ToList();
            }

            public void ReportIssue(MemberDeclarationSyntax memberDeclaration)
            {
                switch (context.SemanticModel.GetDeclaredSymbol(memberDeclaration))
                {
                    case IMethodSymbol methodSymbol:
                        ReportMethodIssue(memberDeclaration, methodSymbol);
                        break;
                    case IPropertySymbol propertySymbol:
                        ReportPropertyIssue(memberDeclaration, propertySymbol);
                        break;
                }
            }

            private void ReportMethodIssue(MemberDeclarationSyntax memberDeclaration, IMethodSymbol methodSymbol)
            {
                if (memberDeclaration is MethodDeclarationSyntax methodDeclaration
                    && !methodDeclaration.Modifiers.Any(SyntaxKind.NewKeyword)
                    && allBaseClassMethods.FirstOrDefault(x => IsDecreasingAccess(x.DeclaredAccessibility, methodSymbol.DeclaredAccessibility, false)
                    && IsMatchingSignature(x, methodSymbol)) is { } hidingMethod)
                {
                    context.ReportIssue(Rule, methodDeclaration.Identifier, hidingMethod.ToDisplayString());
                }
            }

            private void ReportPropertyIssue(MemberDeclarationSyntax memberDeclaration, IPropertySymbol propertySymbol)
            {
                if (memberDeclaration is PropertyDeclarationSyntax propertyDeclaration
                    && !propertyDeclaration.Modifiers.Any(SyntaxKind.NewKeyword)
                    && allBaseClassProperties.FirstOrDefault(x => IsDecreasingPropertyAccess(x, propertySymbol, propertySymbol.IsOverride)) is { } hidingProperty)
                {
                    context.ReportIssue(Rule, propertyDeclaration.Identifier, hidingProperty.ToDisplayString());
                }
            }

            private static bool IsSymbolVisibleFromNamespace(ISymbol symbol, INamespaceSymbol ns) =>
                symbol.DeclaredAccessibility != Accessibility.Private
                && (symbol.DeclaredAccessibility != Accessibility.Internal || ns.Equals(symbol.ContainingNamespace));

            private static bool IsDecreasingPropertyAccess(IPropertySymbol baseProperty, IPropertySymbol propertySymbol, bool isOverride)
            {
                if (baseProperty.Name != propertySymbol.Name
                    || !AreParameterTypesEqual(baseProperty.Parameters, propertySymbol.Parameters))
                {
                    return false;
                }

                var baseGetAccess = GetEffectiveDeclaredAccess(baseProperty.GetMethod, baseProperty.DeclaredAccessibility);
                var baseSetAccess = GetEffectiveDeclaredAccess(baseProperty.SetMethod, baseProperty.DeclaredAccessibility);

                var propertyGetAccess = GetEffectiveDeclaredAccess(propertySymbol.GetMethod, baseProperty.DeclaredAccessibility);
                var propertySetAccess = GetEffectiveDeclaredAccess(propertySymbol.SetMethod, baseProperty.DeclaredAccessibility);

                return IsDecreasingAccess(baseGetAccess, propertyGetAccess, isOverride)
                       || IsDecreasingAccess(baseSetAccess, propertySetAccess, isOverride);
            }

            private static Accessibility GetEffectiveDeclaredAccess(ISymbol symbol, Accessibility defaultAccessibility)
            {
                if (symbol == null)
                {
                    return Accessibility.NotApplicable;
                }

                return symbol.DeclaredAccessibility == Accessibility.NotApplicable
                    ? defaultAccessibility
                    : symbol.DeclaredAccessibility;
            }

            private static bool IsMatchingSignature(IMethodSymbol baseMethod, IMethodSymbol methodSymbol) =>
                baseMethod.Name == methodSymbol.Name
                && baseMethod.TypeParameters.Length == methodSymbol.TypeParameters.Length
                && AreParameterTypesEqual(baseMethod.Parameters, methodSymbol.Parameters);

            private static bool AreParameterTypesEqual(IEnumerable<IParameterSymbol> first, IEnumerable<IParameterSymbol> second) =>
                first.Equals(second, AreParameterTypesEqual);

            private static bool AreParameterTypesEqual(IParameterSymbol first, IParameterSymbol second)
            {
                if (first.RefKind != second.RefKind)
                {
                    return false;
                }

                return first.Type.TypeKind == TypeKind.TypeParameter
                    ? second.Type.TypeKind == TypeKind.TypeParameter
                    : Equals(first.Type.OriginalDefinition, second.Type.OriginalDefinition);
            }

            private static bool IsDecreasingAccess(Accessibility baseAccess, Accessibility memberAccess, bool isOverride)
            {
                if (memberAccess == Accessibility.NotApplicable && isOverride)
                {
                    return false;
                }

                return (baseAccess != Accessibility.NotApplicable && memberAccess == Accessibility.Private)
                       || (baseAccess == Accessibility.Public && memberAccess != Accessibility.Public);
            }
        }
    }
}
