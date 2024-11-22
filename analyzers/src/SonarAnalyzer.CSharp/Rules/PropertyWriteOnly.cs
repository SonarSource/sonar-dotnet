/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
    public sealed class PropertyWriteOnly : PropertyWriteOnlyBase<SyntaxKind, PropertyDeclarationSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
        protected override SyntaxKind SyntaxKind => SyntaxKind.PropertyDeclaration;

        protected override bool IsWriteOnlyProperty(PropertyDeclarationSyntax prop)
        {
            var accessors = prop.AccessorList;
            return accessors is {Accessors: {Count: 1}}
                   && accessors.Accessors.First().IsAnyKind(SyntaxKind.SetAccessorDeclaration, SyntaxKindEx.InitAccessorDeclaration)
                   && !prop.Modifiers.Any(SyntaxKind.OverrideKeyword); // the get may be in the base class
        }
    }
}
