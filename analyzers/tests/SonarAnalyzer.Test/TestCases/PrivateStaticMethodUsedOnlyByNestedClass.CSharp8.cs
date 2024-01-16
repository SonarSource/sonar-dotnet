class OuterClass
{
    static void OnlyUsedByNestedInterface() { }                 // Noncompliant
    private protected static void PrivateProtectedMethod() { }  // Compliant - method is not private

    interface INestedInterface
    {
        void Foo()
        {
            OnlyUsedByNestedInterface();
            PrivateProtectedMethod();
        }
    }
}

interface IOuterInterface
{
    static void OnlyUsedByNestedClass() { }                     // Noncompliant

    class NestedClass
    {
        void Foo()
        {
            OnlyUsedByNestedClass();
        }
    }
}
