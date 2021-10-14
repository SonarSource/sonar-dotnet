int y = 6;

y = 5;

SomeClass someClass = new SomeClass() { SomeField1 = new SomeOtherClass() { SomeField2 = 42 } };

if (someClass.SomeField1.SomeField2 == 42) // FN
{
    someClass.SomeField1.SomeField2 = 42;
}

if (someClass is { SomeField1: { SomeField2: 42 } }) // FN
{
    someClass.SomeField1.SomeField2 = 42;
}

if (someClass is { SomeField1.SomeField2: 42 }) // FN
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
