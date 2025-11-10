/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules;

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
            SyntaxKindEx.RecordDeclaration);

    private sealed class IssueReporter
    {
        private readonly IList<IMethodSymbol> allBaseClassMethods;
        private readonly IList<IPropertySymbol> allBaseClassProperties;
        private readonly IList<IEventSymbol> allBaseClassEvents;

        private readonly SonarSyntaxNodeReportingContext context;

        public IssueReporter(ITypeSymbol classSymbol, SonarSyntaxNodeReportingContext context)
        {
            this.context = context;
            var allBaseClassMembers = classSymbol.BaseType
                    .GetSelfAndBaseTypes()
                    .SelectMany(x => x.GetMembers())
                    .Where(x => IsSymbolVisibleFromNamespace(x, classSymbol.ContainingNamespace))
                    .ToList();

            allBaseClassMethods = allBaseClassMembers.OfType<IMethodSymbol>().ToList();
            allBaseClassProperties = allBaseClassMembers.OfType<IPropertySymbol>().ToList();
            allBaseClassEvents = allBaseClassMembers.OfType<IEventSymbol>().ToList();
        }

        public void ReportIssue(MemberDeclarationSyntax memberDeclaration)
        {
            switch (context.Model.GetDeclaredSymbol(memberDeclaration))
            {
                case IMethodSymbol methodSymbol:
                    ReportMethodIssue(memberDeclaration, methodSymbol);
                    break;
                case IPropertySymbol propertySymbol:
                    ReportPropertyIssue(memberDeclaration, propertySymbol);
                    break;
                case IEventSymbol eventSymbol:
                    ReportEventIssue(memberDeclaration, eventSymbol);
                    break;
            }
        }

        private void ReportMethodIssue(MemberDeclarationSyntax memberDeclaration, IMethodSymbol methodSymbol)
        {
            if (memberDeclaration is MethodDeclarationSyntax methodDeclaration
                && !methodDeclaration.Modifiers.Any(SyntaxKind.NewKeyword)
                && allBaseClassMethods.FirstOrDefault(x =>
                    IsDecreasingAccess(x.DeclaredAccessibility, methodSymbol.DeclaredAccessibility, false)
                    && IsMatchingSignature(x, methodSymbol)) is { } hidingMethod)
            {
                context.ReportIssue(Rule, methodDeclaration.Identifier, hidingMethod.ToDisplayString());
            }
        }

        private void ReportPropertyIssue(MemberDeclarationSyntax memberDeclaration, IPropertySymbol propertySymbol)
        {
            if (memberDeclaration is BasePropertyDeclarationSyntax basePropertyDeclaration
                && !basePropertyDeclaration.Modifiers.Any(SyntaxKind.NewKeyword)
                && allBaseClassProperties.FirstOrDefault(x => IsDecreasingPropertyAccess(x, propertySymbol, propertySymbol.IsOverride)) is { } hidingProperty)
            {
                context.ReportIssue(Rule, GetPropertyToken(basePropertyDeclaration), hidingProperty.ToDisplayString());
            }
        }

        private void ReportEventIssue(MemberDeclarationSyntax memberDeclaration, IEventSymbol eventSymbol)
        {
            if (memberDeclaration is EventDeclarationSyntax eventDeclaration
                && !eventDeclaration.Modifiers.Any(SyntaxKind.NewKeyword)
                && allBaseClassEvents.FirstOrDefault(x => IsDecreasingAccess(x.DeclaredAccessibility, eventSymbol.DeclaredAccessibility, false)
                    && x.Name == eventSymbol.Name) is { } hidingEvent)
            {
                context.ReportIssue(Rule, eventDeclaration.Identifier, hidingEvent.ToDisplayString());
            }
        }

        private static SyntaxToken GetPropertyToken(BasePropertyDeclarationSyntax propertyLike) =>
            propertyLike switch
            {
                PropertyDeclarationSyntax property => property.Identifier,
                IndexerDeclarationSyntax indexer => indexer.ThisKeyword,
                _ => propertyLike.GetFirstToken()
            };

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
            if (symbol is null)
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
