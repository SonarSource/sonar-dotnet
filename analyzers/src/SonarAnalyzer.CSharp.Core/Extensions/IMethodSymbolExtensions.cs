/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Core.Extensions;

public static class IMethodSymbolExtensions
{
    extension(IMethodSymbol methodSymbol)
    {
        public bool IsModuleInitializer => methodSymbol.AnyAttributeDerivesFrom(KnownType.System_Runtime_CompilerServices_ModuleInitializerAttribute);

        public bool IsGetTypeCall =>
            methodSymbol.Name == nameof(Type.GetType)
            && !methodSymbol.IsStatic
            && methodSymbol.ContainingType is not null
            && IsObjectOrType(methodSymbol.ContainingType);

        public SyntaxNode ImplementationSyntax => (methodSymbol.PartialImplementationPart ?? methodSymbol).DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
    }

    private static bool IsObjectOrType(ITypeSymbol namedType) =>
        namedType.SpecialType == SpecialType.System_Object
        || namedType.Is(KnownType.System_Type);
}
