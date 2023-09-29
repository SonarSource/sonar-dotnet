using System;

class MyClass(int first, int second, int third, int fourth)
{
    private int first = first; // Compliant
    private int second; // FN

    void MyMethod()
    {
        int third = 1; // FN
        _ = first is object fourth; // FN
    }
}
