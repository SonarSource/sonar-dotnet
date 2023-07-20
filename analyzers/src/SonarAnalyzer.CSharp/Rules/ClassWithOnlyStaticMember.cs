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

using SonarAnalyzer.Constants;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ClassWithOnlyStaticMember : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1118";
        private const string MessageFormat = "{0}";
        private const string MessageFormatConstructor = "Hide this public constructor by making it '{0}'.";
        private const string MessageFormatStaticClass = "Add a '{0}' constructor or the 'static' keyword to the class declaration.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        private static readonly ISet<Accessibility> ProblematicConstructorAccessibility = new HashSet<Accessibility>
        {
            Accessibility.Public,
            Accessibility.Internal
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                c =>
                {
                    var namedType = c.Symbol as INamedTypeSymbol;
                    if (!namedType.IsClass())
                    {
                        return;
                    }

                    CheckClasses(c, namedType);
                    CheckConstructors(c, namedType);
                },
                SymbolKind.NamedType);

        private static void CheckClasses(SonarSymbolReportingContext context, INamedTypeSymbol utilityClass)
        {
            if (!ClassIsRelevant(utilityClass))
            {
                return;
            }

            var reportMessage = string.Format(MessageFormatStaticClass, utilityClass.IsSealed ? SyntaxConstants.Private : SyntaxConstants.Protected);

            foreach (var syntaxReference in utilityClass.DeclaringSyntaxReferences)
            {
                if (syntaxReference.GetSyntax() is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    context.ReportIssue(CreateDiagnostic(Rule, classDeclarationSyntax.Identifier.GetLocation(), reportMessage));
                }
            }
        }

        private static void CheckConstructors(SonarSymbolReportingContext context, INamedTypeSymbol utilityClass)
        {
            if (!ClassQualifiesForIssue(utilityClass) || !HasMembersAndAllAreStaticExceptConstructors(utilityClass))
            {
                return;
            }

            var reportMessage = string.Format(MessageFormatConstructor, utilityClass.IsSealed ? SyntaxConstants.Private : SyntaxConstants.Protected);

            foreach (var constructor in utilityClass.GetMembers()
                                                    .Where(IsConstructor)
                                                    .Where(symbol => ProblematicConstructorAccessibility.Contains(symbol.DeclaredAccessibility)))
            {
                var syntaxReferences = constructor.DeclaringSyntaxReferences;
                foreach (var syntaxReference in syntaxReferences)
                {
                    if (syntaxReference.GetSyntax() is ConstructorDeclarationSyntax constructorDeclaration)
                    {
                        context.ReportIssue(CreateDiagnostic(Rule, constructorDeclaration.Identifier.GetLocation(), reportMessage));
                    }
                }
            }
        }

        private static bool ClassIsRelevant(INamedTypeSymbol @class) =>
            ClassQualifiesForIssue(@class)
            && HasOnlyQualifyingMembers(@class, @class.GetMembers().Where(member => !member.IsImplicitlyDeclared).ToList());

        private static bool ClassQualifiesForIssue(INamedTypeSymbol @class) =>
            !@class.IsStatic
            && !@class.IsAbstract
            && !@class.AllInterfaces.Any()
            && @class.BaseType.Is(KnownType.System_Object);

        private static bool HasOnlyQualifyingMembers(INamedTypeSymbol @class, IList<ISymbol> members) =>
            members.Any()
            && members.All(member => member.IsStatic)
            && !ClassUsedAsInstanceInMembers(@class, members);

        private static bool ClassUsedAsInstanceInMembers(INamedTypeSymbol @class, IList<ISymbol> members) =>
            members.OfType<IMethodSymbol>().Any(member => @class.Equals(member.ReturnType) || member.Parameters.Any(parameter => @class.Equals(parameter.Type)))
            || members.OfType<IPropertySymbol>().Any(member => @class.Equals(member.Type))
            || members.OfType<IFieldSymbol>().Any(member => @class.Equals(member.Type));

        private static bool HasMembersAndAllAreStaticExceptConstructors(INamedTypeSymbol @class)
        {
            var membersExceptConstructors = @class.GetMembers()
                .Where(member => !IsConstructor(member))
                .ToList();

            return HasOnlyQualifyingMembers(@class, membersExceptConstructors);
        }

        private static bool IsConstructor(ISymbol member) =>
            member is IMethodSymbol {MethodKind: MethodKind.Constructor, IsImplicitlyDeclared: false};
    }
}
