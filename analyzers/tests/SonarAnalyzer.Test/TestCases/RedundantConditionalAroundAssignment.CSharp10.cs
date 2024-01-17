int y = 6;

y = y switch
{
    not 5 => 5 // Noncompliant
};

SomeClass someClass = new SomeClass() { SomeField1 = new SomeOtherClass() { SomeField2 = 42 } };

if (someClass.SomeField1.SomeField2 != 42) // Noncompliant
{
    someClass.SomeField1.SomeField2 = 42;
}

if (someClass is { SomeField1: { SomeField2: not 42 } }) // FN (is expression not supported yet)
{
    someClass.SomeField1.SomeField2 = 42;
}

if (someClass is { SomeField1.SomeField2: not 42 }) // FN (is expression not supported yet)
{
    someClass.SomeField1.SomeField2 = 42;
}

public class SomeClass
{
    public SomeOtherClass SomeField1;
}

public class SomeOtherClass
{
    public int SomeField2;
}
