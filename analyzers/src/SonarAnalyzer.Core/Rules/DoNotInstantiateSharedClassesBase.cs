/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules
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
