using System;

public class Class(string param1, int param2);                  // Noncompliant {{Do not use primary constructors.}}
//                ^^^^^^^^^^^^^^^^^^^^^^^^^^^
public abstract class AbstractClass();                          // Noncompliant
//                                 ^^
public sealed class SealedClass(string param1, int param2);     // Noncompliant
public class Box<T>(string param1, int param2);                 // Noncompliant
public partial class PartialClass(string param1, int param2);   // Noncompliant
public partial class PartialClass;

class BothConstructor(string param1, int param2)                // Noncompliant
{
    BothConstructor(int param2) : this("", param2) { }
}

public struct Struct(string param1, int param2);                            // Noncompliant
//                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^
public readonly struct ReadonlyStruct(string param1, int param2);           // Noncompliant
public ref struct RefStruct(string param1, int param2);                     // Noncompliant
public readonly ref struct ReadonlyRefStruct(string param1, int param2);    // Noncompliant

public record class RecordClass(string param1, int param2);
public record struct RecordStruct(string param1, int param2);
public record Record(string param1, int param2);

public static class StaticClass(string param1, int param2);     // Noncompliant
                                                                // Error@-1 [CS0710]
public interface Interface(string param1, int param2);          // Error [CS9122]

class NormalConstructor
{
    delegate string Delegate(string param1, int param2);

    NormalConstructor(string param1, int param2)
    {
        Delegate _ = (string param1, int param2) => "";
    }

    void Method(string param1, int param2)
    {
        string Local(string param1, int param2) => "";
    }
}
