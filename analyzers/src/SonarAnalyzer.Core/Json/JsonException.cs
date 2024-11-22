/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Json
{
    [Serializable]
    public sealed class JsonException : Exception
    {
        public JsonException(string message, LinePosition position) : base($"{message} at line {position.Line + 1} position {position.Character + 1}") { }

        [ExcludeFromCodeCoverage]
        private JsonException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
