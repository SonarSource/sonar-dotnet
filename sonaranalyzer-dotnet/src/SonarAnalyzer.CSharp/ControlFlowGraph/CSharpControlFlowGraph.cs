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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    public static class CSharpControlFlowGraph
    {
        private static readonly ISet<string> UNSUPPORTED_SYNTAX_NODE_KINDS = new HashSet<string>
        {
           "DeclarationExpression",
           "DefaultLiteralExpression",
           "LocalFunctionStatement",
           "ForEachVariableStatement",
           "ThrowExpression",
           "TupleExpression",
        }.ToImmutableHashSet();

        public static IControlFlowGraph Create(CSharpSyntaxNode node, SemanticModel semanticModel)
        {
            try
            {
                return new CSharpControlFlowGraphBuilder(node, semanticModel).Build();
            }
            catch (Exception exc)
            {
                var location = node?.GetLocation();
                var sb = new StringBuilder();
                sb.AppendLine($"Error creating CFG at {location?.GetLineSpan().Path}:{location?.GetLineNumberToReport()}");
                sb.AppendLine($"Inner exception: {exc.ToString()}");
                throw new ControlFlowGraphException(ExceptionHelper.OneLineReportToPreventRoslynTruncation(sb.ToString()), exc);
            }
        }

        public static bool TryGet(CSharpSyntaxNode node, SemanticModel semanticModel, out IControlFlowGraph cfg)
        {
            cfg = null;
            try
            {
                if (node != null)
                {
                    cfg = Create(node, semanticModel);
                }
                else
                {
                    return false;
                }
            }
            catch (ControlFlowGraphException exc)
            {
                if (exc.InnerException is NotSupportedException notSupportedException &&
                    UNSUPPORTED_SYNTAX_NODE_KINDS.Contains(notSupportedException.Message))
                {
                    return false;
                }
                throw exc;
            }

            return cfg != null;
        }
    }
}
