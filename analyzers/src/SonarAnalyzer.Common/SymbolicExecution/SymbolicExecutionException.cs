/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;

namespace SonarAnalyzer.SymbolicExecution
{
    [Serializable]
    public sealed class SymbolicExecutionException : Exception
    {
        public SymbolicExecutionException() { }

        public SymbolicExecutionException(string message) : base(message) { }

        public SymbolicExecutionException(string message, Exception innerException) : base(message, innerException) { }

        public SymbolicExecutionException(Exception ex, ISymbol symbol, Location location) : base(Serialize(ex, symbol, location), ex) { }

        [ExcludeFromCodeCoverage]
        private SymbolicExecutionException(SerializationInfo info, StreamingContext context) : base(info, context) { } // Fixes S3925

        private static string Serialize(Exception ex, ISymbol symbol, Location location)
        {
            // Roslyn/MSBuild is currently cutting exception message at the end of the line instead
            // of displaying the full message. As a workaround, we replace the line ending with ' ## '.
            // See https://github.com/dotnet/roslyn/issues/1455 and https://github.com/dotnet/roslyn/issues/24346
            var sb = new StringBuilder();
            sb.AppendLine($"Error processing method: {symbol?.Name ?? "{unknown}"}");
            sb.AppendLine($"Method file: {location?.GetLineSpan().Path ?? "{unknown}"}");
            sb.AppendLine($"Method line: {location?.GetLineSpan().StartLinePosition.ToString() ?? "{unknown}"}");
            sb.Append($"Inner exception: {ex}");
            return sb.ToString().Replace(Environment.NewLine, " ## ");
        }
    }
}
