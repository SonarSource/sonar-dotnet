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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RequireAttributeUsageAttribute : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S3993";
    private const string MessageFormat = "Specify AttributeUsage on '{0}'{1}.";

    private static readonly DiagnosticDescriptor Rule =
        DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
        {
            var classDeclaration = (ClassDeclarationSyntax)c.Node;

            if (c.SemanticModel.GetDeclaredSymbol(classDeclaration) is { IsAbstract: false } classSymbol
                && classSymbol.DerivesFrom(KnownType.System_Attribute)
                && !classSymbol.HasAttribute(KnownType.System_AttributeUsageAttribute))
            {
                var additionalText = InheritsAttributeUsage(classSymbol)
                    ? " to improve readability, even though it inherits it from its base type"
                    : string.Empty;

                c.ReportIssue(Rule, classDeclaration.Identifier, classSymbol.Name, additionalText);
            }
        },
        SyntaxKind.ClassDeclaration);

    private static bool InheritsAttributeUsage(INamedTypeSymbol classSymbol) =>
        classSymbol.GetSelfAndBaseTypes()
            // System.Attribute already has AttributeUsage, we don't want to report it
            .TakeWhile(x => !x.Is(KnownType.System_Attribute))
            .Any(x => x.HasAttribute(KnownType.System_AttributeUsageAttribute));
}
