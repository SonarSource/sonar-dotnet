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

namespace SonarAnalyzer.Core.Facade;

/// <summary>Kinds of type declarations. Kinds that do not exist in a language are ignored by its facade.</summary>
[Flags]
public enum TypeDeclarationKinds
{
    /// <summary>No type declaration. Not valid for <see cref="ISyntaxKindFacade{TSyntaxKind}.TypeDeclarations"/>, nor are undefined values.</summary>
    None = 0,

    /// <summary><c>class</c> declaration.</summary>
    Class = 1 << 0,

    /// <summary><c>struct</c> declaration.</summary>
    Struct = 1 << 1,

    /// <summary><c>record</c> and <c>record class</c> declaration.</summary>
    Record = 1 << 2,

    /// <summary><c>record struct</c> declaration.</summary>
    RecordStruct = 1 << 3,

    /// <summary><c>union</c> declaration (C# 15).</summary>
    Union = 1 << 4,

    /// <summary><c>interface</c> declaration.</summary>
    Interface = 1 << 5,

    /// <summary><c>enum</c> declaration.</summary>
    Enum = 1 << 6,

    /// <summary><c>delegate</c> declaration. It has no members, so rules that walk members must not include it.</summary>
    Delegate = 1 << 7,

    /// <summary><c>extension</c> block (C# 14). Not part of <see cref="ClassOrStructLike"/>.</summary>
    Extension = 1 << 8,

    /// <summary>VB <c>Module</c> declaration. Not part of <see cref="ClassLike"/>.</summary>
    Module = 1 << 9,

    /// <summary>Declarations whose symbol is a class.</summary>
    ClassLike = Class | Record,

    /// <summary>Declarations whose symbol is a struct.</summary>
    StructLike = Struct | RecordStruct | Union,

    /// <summary><see cref="ClassLike"/> and <see cref="StructLike"/>.</summary>
    ClassOrStructLike = ClassLike | StructLike,

    /// <summary>
    /// Every type declaration. Includes delegate (not a <c>BaseTypeDeclarationSyntax</c>) and C# 14 extension blocks (a <c>TypeDeclarationSyntax</c> without an identifier),
    /// so check the kind before casting matched nodes.
    /// </summary>
    All = ClassOrStructLike | Interface | Enum | Delegate | Extension | Module,
}
