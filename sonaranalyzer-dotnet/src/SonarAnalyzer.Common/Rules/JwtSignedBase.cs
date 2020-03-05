/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System.Collections.Immutable;

namespace SonarAnalyzer.Rules
{
    public abstract class JwtSignedBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S5659";
        private const string MessageFormat = "Use only strong cipher algorithms when {0} this JWT.";
        private const string MessageVerifying = "verifying the signature of";

        private readonly DiagnosticDescriptor rule;
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected JwtSignedBase(System.Resources.ResourceManager rspecResources)
        {
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources);
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            //CSharpInvocationTracker
            //context.RegisterSyntaxNodeActionInNonGenerated(c =>
            //    {
            //        //var node = c.Node;
            //        //if (true)
            //        //{
            //        //    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, node.GetLocation()));
            //        //}
            //    },
            //    SyntaxKind.InvocationExpression);
        }

    }
}

