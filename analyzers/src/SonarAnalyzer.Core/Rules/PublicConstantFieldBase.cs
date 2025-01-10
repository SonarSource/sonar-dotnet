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

namespace SonarAnalyzer.Rules
{
    public abstract class PublicConstantFieldBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S2339";
        protected const string MessageFormat = "Change this constant to a {0} property.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class PublicConstantFieldBase<TLanguageKindEnum, TFieldDeclarationSyntax, TFieldName>
        : PublicConstantFieldBase
        where TLanguageKindEnum : struct
        where TFieldDeclarationSyntax : SyntaxNode
        where TFieldName : SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                GeneratedCodeRecognizer,
                c =>
                {
                    var field = (TFieldDeclarationSyntax)c.Node;
                    var variables = GetVariables(field).ToList();

                    if (!variables.Any())
                    {
                        return;
                    }

                    var anyVariable = variables.First();
                    if (!(c.SemanticModel.GetDeclaredSymbol(anyVariable) is IFieldSymbol symbol) ||
                        !symbol.IsConst ||
                        symbol.GetEffectiveAccessibility() != Accessibility.Public)
                    {
                        return;
                    }

                    foreach (var variable in variables)
                    {
                        c.ReportIssue(SupportedDiagnostics[0], GetReportLocation(variable), MessageArgument);
                    }
                },
                FieldDeclarationKind);
        }

        protected abstract IEnumerable<TFieldName> GetVariables(TFieldDeclarationSyntax node);

        public abstract TLanguageKindEnum FieldDeclarationKind { get; }
        public abstract string MessageArgument { get; }

        protected abstract Location GetReportLocation(TFieldName node);
    }
}
