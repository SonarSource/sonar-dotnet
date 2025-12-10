using System.Runtime.InteropServices;

public record OptionalRefOutParameter
{
    public void WithRefParam([Optional] ref int i) { } // Noncompliant
//                            ^^^^^^^^

    public void WithOutParam([Optional] out int i) // Noncompliant {{Remove the 'Optional' attribute, it cannot be used with 'out'.}}
    {
        i = 23;
    }

    public void Compliant([Optional] int i) { }
}
