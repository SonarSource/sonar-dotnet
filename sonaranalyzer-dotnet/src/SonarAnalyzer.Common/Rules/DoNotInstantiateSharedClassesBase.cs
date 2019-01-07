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

using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class DoNotInstantiateSharedClassesBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4277";
        protected const string MessageFormat = "Refactor this code so that it doesn't invoke the constructor of this class.";

        protected static bool IsShared(AttributeData data)
        {
            // This is equivalent to System.ComponentModel.Composition.CreationPolicy.Shared,
            // but we do not want dependency on System.ComponentModel.Composition just for that.
            const int CreationPolicy_Shared = 1;

            return data.ConstructorArguments.Any(arg =>
                    arg.Type.Is(KnownType.System_ComponentModel_Composition_CreationPolicy) &&
                    Equals(arg.Value, CreationPolicy_Shared));
        }
    }
}
