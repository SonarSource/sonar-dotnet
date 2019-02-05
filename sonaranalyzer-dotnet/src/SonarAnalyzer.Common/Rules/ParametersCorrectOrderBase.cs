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

using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ParametersCorrectOrderBase<TArgumentSyntax> : SonarDiagnosticAnalyzer
        where TArgumentSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S2234";
        protected const string MessageFormat = "Parameters to '{0}' have the same names but not the same order as the method arguments.";

        protected class IdentifierArgument
        {
            public string IdentifierName { get; set; }
            public TArgumentSyntax ArgumentSyntax { get; set; }
        }

        protected class PositionalIdentifierArgument : IdentifierArgument
        {
            public int Position { get; set; }
        }

        protected class NamedIdentifierArgument : IdentifierArgument
        {
            public string DeclaredName { get; set; }
        }
    }
}

