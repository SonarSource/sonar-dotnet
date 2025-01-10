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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class EncryptionAlgorithmsShouldBeSecure : EncryptionAlgorithmsShouldBeSecureBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        public EncryptionAlgorithmsShouldBeSecure() : base(AnalyzerConfiguration.AlwaysEnabled) { }

        protected override TrackerBase<SyntaxKind, PropertyAccessContext>.Condition IsInsideObjectInitializer() =>
            context => context.Node.FirstAncestorOrSelf<ObjectMemberInitializerSyntax>() != null;

        protected override TrackerBase<SyntaxKind, InvocationContext>.Condition HasPkcs1PaddingArgument() =>
            (context) =>
            {
                var argumentList = ((InvocationExpressionSyntax)context.Node).ArgumentList;
                var values = argumentList.ArgumentValuesForParameter(context.Model, "padding");
                return values.Length == 1
                    && values[0] is ExpressionSyntax valueSyntax
                    && context.Model.GetSymbolInfo(valueSyntax).Symbol is ISymbol symbol
                    && symbol.Name == "Pkcs1";
            };
    }
}
