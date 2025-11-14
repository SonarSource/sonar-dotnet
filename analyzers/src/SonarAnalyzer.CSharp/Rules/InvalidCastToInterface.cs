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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InvalidCastToInterface : InvalidCastToInterfaceBase<SyntaxKind>
{
    public static readonly DiagnosticDescriptor S1944 = DescriptorFactory.Create(DiagnosticId, MessageFormat);  // This indirection is needed only because of the old SE engine, see base class.

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
    protected override DiagnosticDescriptor Rule => S1944;
}
