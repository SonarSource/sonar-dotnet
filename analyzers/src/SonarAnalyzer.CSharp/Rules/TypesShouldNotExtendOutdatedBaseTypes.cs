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
                c.ReportIssue(Rule, classDeclaration.Identifier, messageArgs: classSymbol.BaseType.ToDisplayString());
            }
        },
        // The rule is not applicable for records as at the current moment all the outdated types are classes and records cannot inherit classes.
        SyntaxKind.ClassDeclaration);
}
