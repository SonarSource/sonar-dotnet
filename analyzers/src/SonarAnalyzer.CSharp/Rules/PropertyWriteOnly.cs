/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
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
                   && accessors.Accessors.First().Kind() is SyntaxKind.SetAccessorDeclaration or SyntaxKindEx.InitAccessorDeclaration
                   && !prop.Modifiers.Any(SyntaxKind.OverrideKeyword); // the get may be in the base class
        }
    }
}
