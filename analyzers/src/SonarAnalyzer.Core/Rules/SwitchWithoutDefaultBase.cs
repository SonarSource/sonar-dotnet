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
    public abstract class SwitchWithoutDefaultBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S131";
        protected const string MessageFormat = "Add a '{0}' clause to this '{1}' statement.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class SwitchWithoutDefaultBase<TLanguageKindEnum> : SwitchWithoutDefaultBase
        where TLanguageKindEnum : struct
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                GeneratedCodeRecognizer,
                c =>
                {
                    if (TryGetDiagnostic(c.Node, out var diagnostic))
                    {
                        c.ReportIssue(diagnostic);
                    }
                },
                SyntaxKindsOfInterest.ToArray());
        }

        protected abstract bool TryGetDiagnostic(SyntaxNode node, out Diagnostic diagnostic);

        public abstract ImmutableArray<TLanguageKindEnum> SyntaxKindsOfInterest { get; }
    }
}
