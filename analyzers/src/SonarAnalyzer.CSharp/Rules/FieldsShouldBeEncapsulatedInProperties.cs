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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FieldsShouldBeEncapsulatedInProperties : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S1104";
    private const string MessageFormat = "Make this field 'private' and encapsulate it in a 'public' property.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ISet<SyntaxKind> ValidModifiers = new HashSet<SyntaxKind>
    {
        SyntaxKind.PrivateKeyword,
        SyntaxKind.ProtectedKeyword,
        SyntaxKind.InternalKeyword,
        SyntaxKind.ReadOnlyKeyword,
        SyntaxKind.ConstKeyword
    };

    private static readonly ImmutableArray<KnownType> IgnoredTypes = ImmutableArray.Create(
        KnownType.UnityEngine_MonoBehaviour,
        KnownType.UnityEngine_ScriptableObject);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var fieldDeclaration = (FieldDeclarationSyntax)c.Node;
                if (fieldDeclaration.Modifiers.Any(m => ValidModifiers.Contains(m.Kind())))
                {
                    return;
                }

                var firstVariable = fieldDeclaration.Declaration.Variables[0];
                var symbol = c.Model.GetDeclaredSymbol(firstVariable);
                var parentSymbol = c.Model.GetDeclaredSymbol(fieldDeclaration.Parent);
                if (symbol.ContainingType.DerivesFromAny(IgnoredTypes)
                    || parentSymbol.HasAttribute(KnownType.System_Runtime_InteropServices_StructLayoutAttribute)
                    || Serializable(symbol, parentSymbol))
                {
                    return;
                }

                if (symbol.GetEffectiveAccessibility() == Accessibility.Public)
                {
                    c.ReportIssue(Rule, firstVariable);
                }
            },
            SyntaxKind.FieldDeclaration);

    private static bool Serializable(ISymbol symbol, ISymbol parentSymbol) =>
        parentSymbol.HasAttribute(KnownType.System_SerializableAttribute)
        && !symbol.HasAttribute(KnownType.System_NonSerializedAttribute);
}
