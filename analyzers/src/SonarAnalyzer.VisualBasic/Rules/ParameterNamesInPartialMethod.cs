/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class ParameterNamesInPartialMethod : ParameterNamesInPartialMethodBase<SyntaxKind, MethodBlockBaseSyntax>
    {
        public ParameterNamesInPartialMethod() : base(RspecStrings.ResourceManager) { }

        protected override LanguageFacade LanguageFacade => VisualBasicFacade.Instance;
        protected override SyntaxKind[] SyntaxKinds { get; } = new[] { SyntaxKind.SubBlock, SyntaxKind.FunctionBlock };

        protected override IEnumerable<SyntaxToken> ParameterIdentifiers(MethodBlockBaseSyntax method) =>
            method.BlockStatement.ParameterList.Parameters.Select(x => x.Identifier.Identifier);
    }
}
