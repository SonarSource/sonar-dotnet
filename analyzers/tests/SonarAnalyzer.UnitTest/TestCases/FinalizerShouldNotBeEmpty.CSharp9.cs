record Record1
{
    ~Record1() // Noncompliant {{Remove this empty finalizer.}}
    {

    }
}

record Record2
{
    ~Record2() // Noncompliant {{Remove this empty finalizer.}}
    {
        // Some comment
    }
}

record Record3
{
    ~Record3() // Compliant
    {
        var something = 0;
    }
}

record Record4
{
    bool value;

    ~Record4() => value = false;
}
