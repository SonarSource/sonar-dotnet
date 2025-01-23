/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ComparableInterfaceImplementation : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1210";
        private const string MessageFormat = "When implementing {0}, you should also override {1}.";
        private const string ObjectEquals = nameof(object.Equals);

        private static readonly ImmutableArray<KnownType> ComparableInterfaces =
            ImmutableArray.Create(
                KnownType.System_IComparable,
                KnownType.System_IComparable_T);

        private static readonly ComparisonKind[] ComparisonKinds =
           Enum.GetValues(typeof(ComparisonKind)).Cast<ComparisonKind>()
               .Where(x => x != ComparisonKind.None)
               .OrderBy(x => x)
               .ToArray();

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var classDeclaration = (TypeDeclarationSyntax)c.Node;

                    var classSymbol = c.Model.GetDeclaredSymbol(classDeclaration);
                    if (classSymbol == null)
                    {
                        return;
                    }

                    var baseImplementsIComparable = classSymbol
                        .BaseType
                        .GetSelfAndBaseTypes()
                        .Any(t => t.ImplementsAny(ComparableInterfaces));
                    if (baseImplementsIComparable)
                    {
                        return;
                    }

                    var implementedComparableInterfaces = GetImplementedComparableInterfaces(classSymbol);
                    if (!implementedComparableInterfaces.Any())
                    {
                        return;
                    }

                    var classMembers = classSymbol.GetMembers().OfType<IMethodSymbol>();

                    var membersToOverride = GetMembersToOverride(classMembers).ToList();

                    if (membersToOverride.Any())
                    {
                        c.ReportIssue(
                            Rule,
                            classDeclaration.Identifier,
                            string.Join(" or ", implementedComparableInterfaces),
                            membersToOverride.JoinAnd());
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration);

        private static IEnumerable<string> GetImplementedComparableInterfaces(INamedTypeSymbol classSymbol) =>
            classSymbol
            .Interfaces
            .Where(i => i.OriginalDefinition.IsAny(ComparableInterfaces))
            .Select(GetClassNameOnly)
            .ToList();

        private static string GetClassNameOnly(INamedTypeSymbol typeSymbol) =>
            typeSymbol.OriginalDefinition.ToDisplayString()
                .Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Last();

        private static IEnumerable<string> GetMembersToOverride(IEnumerable<IMethodSymbol> methods)
        {
            if (!methods.Any(KnownMethods.IsObjectEquals))
            {
                yield return ObjectEquals;
            }

            var overridenOperators = methods
                .Where(m => m.MethodKind == MethodKind.UserDefinedOperator)
                .Select(m => m.ComparisonKind());

            foreach (var comparisonKind in ComparisonKinds.Except(overridenOperators))
            {
                yield return CSharp(comparisonKind);
            }
        }

        private static string CSharp(ComparisonKind kind) =>
            kind switch
            {
                ComparisonKind.Equals => "==",
                ComparisonKind.NotEquals => "!=",
                ComparisonKind.LessThan => "<",
                ComparisonKind.LessThanOrEqual => "<=",
                ComparisonKind.GreaterThan => ">",
                ComparisonKind.GreaterThanOrEqual => ">=",
                _ => throw new InvalidOperationException(),
            };
    }
}
