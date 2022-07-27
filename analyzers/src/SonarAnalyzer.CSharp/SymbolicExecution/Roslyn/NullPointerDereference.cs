/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp
{
    public class NullPointerDereference : NullPointerDereferenceBase
    {
        private const string MessageFormat = "'{0}' is null on at least one execution path.";

        internal static readonly DiagnosticDescriptor S2259 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        protected override DiagnosticDescriptor Rule => S2259;

        public override bool ShouldExecute() => true;

        protected override ProgramState PostProcessSimple(SymbolicContext context)
        {
            return base.PostProcessSimple(context);
        }
    }
}
