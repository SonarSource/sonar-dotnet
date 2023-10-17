using System;

public class MyWrongClass(int p1, int p2, int p3, int p4) { } // Noncompliant {{Constructor has 4 parameters, which is greater than the 3 authorized.}}

public class SubClass(int p1, int p2, int p3, int p4) : MyWrongClass(p1, p2, p3, p4) { } // Compliant: base class requires them

public class SubClass2() : MyWrongClass(1, 2, 3, 4) // Compliant
{
    public SubClass2(int p1, int p2, int p3, int p4, int p5) : this() { } // Noncompliant

    void Method()
    {
        var a = (int p1 = 1, int p2 = 2, int p3 = 3, int p4 = 4) => true; // Noncompliant {{Lambda has 4 parameters, which is greater than the 3 authorized.}}
    }
}

public struct MyWrongStruct(int p1, int p2, int p3, int p4) { } // Noncompliant {{Constructor has 4 parameters, which is greater than the 3 authorized.}}
