using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal class UseToPortableExecutableReference
{
    public void Test(Compilation compilation, CSharpCompilation csharpCompilation)
    {
        _ = compilation.ToMetadataReference();          // Noncompliant {{Use 'ToPortableExecutableReference()' instead of 'ToMetadataReference'. 'ToMetadataReference' exposes source symbols and locations, as VS project references do; the replacement loads real PE symbols, as command-line builds do. Suppress with a reason if the test needs source symbols.}}
        //              ^^^^^^^^^^^^^^^^^^^
        _ = csharpCompilation.ToMetadataReference();    // Noncompliant
        _ = new Other().ToMetadataReference();          // Compliant
        _ = MetadataReference.CreateFromImage([]);      // Compliant
#pragma warning disable T0047 // Needs source locations of the referenced compilation
        _ = compilation.ToMetadataReference();
#pragma warning restore T0047
    }

    private class Other
    {
        public object ToMetadataReference() => null;
    }
}
