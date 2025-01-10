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
    public sealed class StaticFieldWrittenFromInstanceMember : StaticFieldWrittenFrom
    {
        private const string DiagnosticId = "S2696";
        private const string MessageFormat = "{0}";
        private const string MessageFormatMultipleOptions = "Make the enclosing instance {0} 'static' or remove this set on the 'static' field.";
        private const string MessageFormatRemoveSet = "Remove this set, which updates a 'static' field from an instance {0}.";

        protected override DiagnosticDescriptor Rule =>
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        protected override bool IsValidCodeBlockContext(SyntaxNode node, ISymbol owningSymbol) =>
            owningSymbol is { IsStatic: false }
            && node is MethodDeclarationSyntax or AccessorDeclarationSyntax;

        protected override string GetDiagnosticMessageArgument(SyntaxNode node, ISymbol owningSymbol, IFieldSymbol field)
        {
            var messageFormat = owningSymbol.IsChangeable()
                               ? MessageFormatMultipleOptions
                               : MessageFormatRemoveSet;
            var declarationType = GetDeclarationType(node);

            return string.Format(messageFormat, declarationType);
        }

        private static string GetDeclarationType(SyntaxNode declaration) =>
            declaration switch
            {
                MethodDeclarationSyntax => "method",
                AccessorDeclarationSyntax => "property",
                _ => throw new NotSupportedException($"Not expected syntax kind {declaration.RawKind}.")
            };
    }
}
