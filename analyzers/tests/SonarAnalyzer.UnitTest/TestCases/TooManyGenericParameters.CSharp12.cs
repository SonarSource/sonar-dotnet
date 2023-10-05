class Bar<T1, T2, T3>(string classParam) // Noncompliant
{
    void Method()
    {
        bool GenericLambda<T1, T2, T3, T4>(T1 lambdaParam = default(T1)) => true; // Noncompliant
    }
}
