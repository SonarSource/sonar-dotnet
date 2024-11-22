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

namespace SonarAnalyzer.Rules
{
    public abstract class FunctionNestingDepthBase : ParametrizedDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S134";
        private const string MessageFormat = "Refactor this code to not nest more than {0} control flow statements.";
        private const int DefaultValueMaximum = 3;

        protected readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade Language { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        [RuleParameter("maximumNestingLevel", PropertyType.Integer, "Maximum allowed control flow statement nesting depth.", DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        protected FunctionNestingDepthBase() =>
            rule = Language.CreateDescriptor(DiagnosticId, MessageFormat, isEnabledByDefault: false);

        protected class NestingDepthCounter
        {
            private readonly int maximumNestingDepth;
            private readonly Action<SyntaxToken> actionMaximumExceeded;
            private int currentDepth;

            public NestingDepthCounter(int maximumNestingDepth, Action<SyntaxToken> actionMaximumExceeded)
            {
                this.maximumNestingDepth = maximumNestingDepth;
                this.actionMaximumExceeded = actionMaximumExceeded;
            }

            public void CheckNesting(SyntaxToken keyword, Action visitAction)
            {
                currentDepth++;
                if (currentDepth <= maximumNestingDepth)
                {
                    visitAction();
                }
                else
                {
                    actionMaximumExceeded(keyword);
                }
                currentDepth--;
            }
        }
    }
}
