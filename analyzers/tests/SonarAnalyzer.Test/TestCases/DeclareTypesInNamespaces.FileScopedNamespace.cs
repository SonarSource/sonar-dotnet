namespace FileScopedNamespace;

class Foo { }   // Compliant

union UnionInFileScopedNamespace(int, string)
{
    class InnerClassOfUnion { }     // Compliant NET-4916
}
