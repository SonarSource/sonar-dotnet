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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.TestFramework.Extensions;

public static class CompilationExtensions
{
    public static string LanguageVersionString(this Compilation compilation) =>
        compilation switch
        {
            CSharpCompilation cs => cs.LanguageVersion.ToString(),
            VisualBasicCompilation vb => vb.LanguageVersion.ToString(),
            _ => throw new NotSupportedException($"Not supported compilation type: '{compilation?.GetType()}'")
        };

    public static byte[] EmitToImage(this Compilation compilation)
    {
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        result.Success.Should().BeTrue("the compilation should emit an assembly. Errors: {0}", string.Join(Environment.NewLine, result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error)));
        return stream.ToArray();
    }

    // Unlike Compilation.ToMetadataReference(), which exposes the source symbols of this compilation, this loads PE symbols from the emitted image.
    public static PortableExecutableReference ToPortableExecutableReference(this Compilation compilation) =>
        MetadataReference.CreateFromImage(compilation.EmitToImage());
}
