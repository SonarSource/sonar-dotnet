using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static DelegateSubtraction;

public class DelegateSubtraction
{
    public delegate void MyDelegate();

    private static void Test()
    {
        MyDelegate first, second, third, fourth;
        first = () => Console.Write("1");
        second = () => Console.Write("2");
        third = () => Console.Write("3");
        fourth = () => Console.Write("4");

        MyDelegate chain1234 = first + second + third + fourth; // Compliant - chain sequence = "1234"
        MyDelegate chain12 = chain1234 - third - fourth; // Compliant - chain sequence = "12"
        chain12 = chain1234 - (third + third) - fourth; // Noncompliant {{Review this subtraction of a chain of delegates: it may not work as you expect.}}
//                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        // The chain sequence of "chain23" will be "1234" instead of "23"!
        // Indeed, the sequence "1234" does not contain the subsequence "14", so nothing is subtracted
        // (but note that "1234" contains both the "1" and "4" subsequences)
        MyDelegate chain23 = chain1234 - (first + fourth); // Noncompliant

        chain23(); // will print "1234"!

        chain23 = chain1234 - first - (fourth); // Compliant - "1" is first removed, followed by "4"

        chain23(); // will print "23"

        chain23 -= first + fourth; // Noncompliant
        chain23 -= (first); // Compliant

        unsafe
        {
            GCHandle pinnedArray = GCHandle.Alloc(new object(), GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();

            int* a = (int*)pointer.ToPointer();
            int* b = (int*)pointer.ToPointer();
            var zero = a - b;
        }
    }
}
record R
{
    MyDelegate first, second, third, fourth;
    string Property
    {
        init
        {
            MyDelegate chain1234 = first + second + third + fourth; // Compliant - chain sequence = "1234"
            MyDelegate chain12 = chain1234 - third - fourth; // Compliant - chain sequence = "12"
            chain12 = chain1234 - (third + third) - fourth; // Noncompliant {{Review this subtraction of a chain of delegates: it may not work as you expect.}}
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8467
class Repro_8467
{
    void SwitchStatement(Action action)
    {
        action -= 1 switch // Noncompliant FP
        {
            1 => action,
            2 => action,
        };
    }
}


public partial class PartialProperty
{
    public partial MyDelegate first  => () => Console.Write("1"); 
    public partial MyDelegate second => () => Console.Write("2");
    public partial MyDelegate third => () => Console.Write("3");
    public partial MyDelegate fourth => () => Console.Write("4");

    public void TestPartialProperties()
    {
        MyDelegate chain1234 = first + second + third + fourth; // Compliant - chain sequence = "1234"
        MyDelegate chain12 = chain1234 - third - fourth; // Compliant - chain sequence = "12"
        chain12 = chain1234 - (third + third) - fourth; // Noncompliant {{Review this subtraction of a chain of delegates: it may not work as you expect.}}
        MyDelegate chain23 = chain1234 - (first + fourth); // Noncompliant

        chain23(); // will print "1234"!

        chain23 = chain1234 - first - (fourth); // Compliant - "1" is first removed, followed by "4"

        chain23(); // will print "23"

        chain23 -= first + fourth; // Noncompliant
        chain23 -= (first); // Compliant
    }
}

public partial class PartialProperty
{
    public partial MyDelegate first { get; }
    public partial MyDelegate second { get; }
    public partial MyDelegate third { get; }
    public partial MyDelegate fourth { get; }
}

