﻿/*
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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StaticFieldInGenericClass : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S2743";
    private const string MessageFormat = "A static field in a generic type is not shared among instances of different close constructed types.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var typeDeclaration = (TypeDeclarationSyntax)c.Node;
                var typeParameterNames = CollectTypeParameterNames(typeDeclaration);
                if (c.IsRedundantPositionalRecordContext()
                    || typeParameterNames.Length == 0
                    || BaseTypeHasGenericTypeArgument(typeDeclaration, typeParameterNames))
                {
                    return;
                }
                var variables = typeDeclaration.Members
                    .OfType<FieldDeclarationSyntax>()
                    .Where(x => x.Modifiers.Any(SyntaxKind.StaticKeyword) && !HasGenericType(c, x.Declaration.Type, typeParameterNames))
                    .SelectMany(x => x.Declaration.Variables);
                foreach (var variable in variables)
                {
                    CheckMember(c, variable, variable.Identifier.GetLocation(), typeParameterNames);
                }
                foreach (var property in typeDeclaration.Members.OfType<PropertyDeclarationSyntax>().Where(x => x.Modifiers.Any(SyntaxKind.StaticKeyword) && x.ExpressionBody is null))
                {
                    CheckMember(c, property, property.Identifier.GetLocation(), typeParameterNames);
                }
            },
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKindEx.RecordStructDeclaration,
            SyntaxKind.StructDeclaration);

    private static void CheckMember(SonarSyntaxNodeReportingContext context, SyntaxNode root, Location location, string[] typeParameterNames)
    {
        if (!HasGenericType(context, root, typeParameterNames))
        {
            context.ReportIssue(Rule, location);
        }
    }

    private static string[] CollectTypeParameterNames(SyntaxNode current)
    {
        var names = new HashSet<string>();
        while (current is not null)
        {
            if (current is TypeDeclarationSyntax { TypeParameterList: not null } typeDeclaration)
            {
                names.AddRange(typeDeclaration.TypeParameterList.Parameters.Select(x => x.Identifier.ValueText));
            }
            current = current.Parent;
        }
        return names.ToArray();
    }

    private static bool HasGenericType(SonarSyntaxNodeReportingContext context, SyntaxNode root, string[] typeParameterNames) =>
        root.DescendantNodesAndSelf()
            .OfType<IdentifierNameSyntax>()
            .Any(x => typeParameterNames.Contains(x.Identifier.Value) && context.Model.GetSymbolInfo(x).Symbol is { Kind: SymbolKind.TypeParameter });

    private static bool BaseTypeHasGenericTypeArgument(TypeDeclarationSyntax typeDeclaration, string[] typeParameterNames) =>
        typeDeclaration.BaseList is { } baseList && baseList.Types.Any(x => x.Type is GenericNameSyntax genericType && HasGenericTypeArgument(genericType, typeParameterNames));

    private static bool HasGenericTypeArgument(GenericNameSyntax genericType, string[] typeParameterNames) =>
        genericType.TypeArgumentList.Arguments.OfType<SimpleNameSyntax>().Any(x => typeParameterNames.Contains(x.Identifier.ValueText));
}
