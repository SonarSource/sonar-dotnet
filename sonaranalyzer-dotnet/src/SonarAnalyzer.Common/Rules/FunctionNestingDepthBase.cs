/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class FunctionNestingDepthBase : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S134";
        internal const string MessageFormat = "Refactor this code to not nest more than {0} control flow statements.";

        internal class NestingDepthCounter
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
                this.currentDepth++;

                if (this.currentDepth <= this.maximumNestingDepth)
                {
                    visitAction();
                }
                else
                {
                    this.actionMaximumExceeded(keyword);
                }

                this.currentDepth--;
            }
        }
    }
}
