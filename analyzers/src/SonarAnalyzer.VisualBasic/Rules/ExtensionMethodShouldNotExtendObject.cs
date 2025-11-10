/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class ExtensionMethodShouldNotExtendObject : ExtensionMethodShouldNotExtendObjectBase<SyntaxKind, MethodStatementSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override bool IsExtensionMethod(MethodStatementSyntax declaration, SemanticModel semanticModel) =>
        declaration.Parent.Parent is ModuleBlockSyntax
        && declaration.AttributeLists.SelectMany(x => x.Attributes).Any(x => x.IsKnownType(KnownType.System_Runtime_CompilerServices_ExtensionAttribute, semanticModel));
}
