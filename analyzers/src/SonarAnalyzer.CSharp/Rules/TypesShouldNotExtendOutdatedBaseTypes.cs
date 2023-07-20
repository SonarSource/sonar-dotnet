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
    public sealed class TypesShouldNotExtendOutdatedBaseTypes : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4052";
        private const string MessageFormat = "Refactor this type not to derive from an outdated type '{0}'.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        private static readonly ImmutableArray<KnownType> OutdatedTypes =
            ImmutableArray.Create(
                KnownType.System_ApplicationException,
                KnownType.System_Xml_XmlDocument,
                KnownType.System_Collections_CollectionBase,
                KnownType.System_Collections_DictionaryBase,
                KnownType.System_Collections_Queue,
                KnownType.System_Collections_ReadOnlyCollectionBase,
                KnownType.System_Collections_SortedList,
                KnownType.System_Collections_Stack);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                var classDeclaration = (ClassDeclarationSyntax)c.Node;
                var classSymbol = (INamedTypeSymbol)c.ContainingSymbol;

                if (!classDeclaration.Identifier.IsMissing
                    && classSymbol.BaseType.IsAny(OutdatedTypes))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, classDeclaration.Identifier.GetLocation(),
                        messageArgs: classSymbol.BaseType.ToDisplayString()));
                }
            },
            // The rule is not applicable for records as at the current moment all the outdated types are classes and records cannot inherit classes.
            SyntaxKind.ClassDeclaration);
    }
}
