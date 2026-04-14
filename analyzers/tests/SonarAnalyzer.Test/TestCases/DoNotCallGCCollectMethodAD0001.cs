// We need to define a custom GC class in the System namespace to simulate the AD0001
namespace System;

public class GC
{
    long GetTotalMemory(); // Error [CS0501]

    void Foo() =>
        GetTotalMemory(); // Noncompliant
}
