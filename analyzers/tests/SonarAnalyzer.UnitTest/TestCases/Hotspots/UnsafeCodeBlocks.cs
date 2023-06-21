using System;

public class Sample
{
    unsafe void MethodScope(byte* pointer) { }                      // Noncompliant {{Make sure that using "unsafe" is safe here.}}
//  ^^^^^^

    void BlockScope()
    {
        unsafe                                                      // Noncompliant
//      ^^^^^^
        {
        }
    }

    void SafeMethod() { }                                           // Compliant

    void LocalFunction()
    {
        unsafe void Local(byte* pointer) { }                        // Noncompliant
        void SafeLocal(byte noPointer) { }                          // Compliant
    }

    unsafe class UnsafeClass { }                                    // Noncompliant

    unsafe struct UnsafeStruct                                      // Noncompliant
    {
        unsafe fixed byte unsafeFixedBuffer[16];                    // Noncompliant
    }

    unsafe interface IUnsafeInterface { }                           // Noncompliant

    unsafe Sample(byte* pointer) { }                                // Noncompliant

    static unsafe Sample() { }                                      // Noncompliant

    unsafe ~Sample() { }                                            // Noncompliant

    unsafe byte* unsafeField;                                       // Noncompliant

    unsafe byte* UnsafeProperty { get; }                            // Noncompliant

    unsafe event EventHandler UnsafeEvent;                          // Noncompliant

    unsafe delegate void UnsafeDelegate(byte* pointer);             // Noncompliant

    unsafe int this[int i] => 5;                                    // Noncompliant

    public unsafe static Sample operator +(Sample other) => other;  // Noncompliant

    // from RSPEC
    public unsafe int SubarraySum(int[] array, int start, int end)  // Noncompliant
    {
        var sum = 0;

        // Skip array bound checks for extra performance
        fixed (int* firstNumber = array)
        {
            for (int i = start; i < end; i++)
                sum += *(firstNumber + i);
        }

        return sum;
    }

    // from C# docs
    unsafe static void SquarePtrParam(int* p)                       // Noncompliant
    {
        *p *= *p;
    }

    unsafe static void Main()                                       // Noncompliant
    {
        int i = 5;
        // Unsafe method: uses address-of operator (&).
        SquarePtrParam(&i);
        Console.WriteLine(i);
    }
}
