dynamic d = 5;
var x = d >> 5.4; // Noncompliant

nuint i = 0;
var y = i >> 4; // Compliant

nint j = 1;
var z = j >> 4; // Compliant

x = d >> new ImplicitCast();
x = d >> new NoImplicitCast(); // Noncompliant

record ImplicitCast
{
    public static implicit operator int(ImplicitCast self) => 1;
}

record NoImplicitCast { }
