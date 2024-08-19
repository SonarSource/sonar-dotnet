using System;

MyClass s = new(1, new int[] { 2, 3 }); // Noncompliant
//                 ^^^^^^^^^^^^^^^^^^
MyClass s1 = new(1, 2, 3); // Compliant

public class MyClass
{
    public MyClass(int a, params int[] args) { }
}
