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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class UsingCommandLineArgumentsBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S4823";
        private const string MessageFormat = "Make sure that command line arguments are used safely here.";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected MethodDeclarationTracker MethodDeclarationTracker { get; set; }

        protected DiagnosticDescriptor Rule { get; }

        protected UsingCommandLineArgumentsBase(System.Resources.ResourceManager rspecResources) =>
             Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources).WithNotConfigurable();

        protected override void Initialize(SonarAnalysisContext context) =>
            MethodDeclarationTracker.Track(context,
                                           MethodDeclarationTracker.IsMainMethod(),
                                           MethodDeclarationTracker.ParameterAtIndexIsUsed(0));
    }
}
