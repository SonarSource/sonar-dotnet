/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class UseArrayEmptyBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S666";
        internal const string MessageFormat = "Declare this empty array using Array.Empty<T>().";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class UseArrayEmptyBase<TLanguageKindEnum, TCreation, TInitialization>: UseArrayEmptyBase
        where TLanguageKindEnum : struct
        where TCreation : SyntaxNode
        where TInitialization : SyntaxNode
    {
        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
               GeneratedCodeRecognizer,
               c =>
               {
                   var node = c.Node;
                   if(IsEmptyCreation(node as TCreation) || IsEmptyInitialization(node as TInitialization))
                   {
                       c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, node.GetLocation()));
                   }
               },
               SyntaxKindsOfInterest.ToArray());
        }

        protected abstract bool IsEmptyCreation(TCreation node);
        protected bool IsEmptyInitialization(TInitialization node) => node != null && !node.ChildNodes().Any();

        protected abstract ImmutableArray<TLanguageKindEnum> SyntaxKindsOfInterest { get; }
        protected abstract DiagnosticDescriptor Rule { get; }
        public override sealed ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
    }
}
