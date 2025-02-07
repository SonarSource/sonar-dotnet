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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Core.Rules
{
    public abstract class TabCharacterBase : SonarDiagnosticAnalyzer
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterTreeAction(
                GeneratedCodeRecognizer,
                c =>
                {
                    var offset = c.Tree.GetText().ToString().IndexOf('\t');
                    if (offset < 0)
                    {
                        return;
                    }

                    var location = c.Tree.GetLocation(TextSpan.FromBounds(offset, offset));
                    c.ReportIssue(SupportedDiagnostics[0], location);
                });
        }

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }
}
