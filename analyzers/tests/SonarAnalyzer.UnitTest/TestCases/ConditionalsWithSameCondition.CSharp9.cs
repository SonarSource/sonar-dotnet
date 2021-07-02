class ConditionalsWithSameCondition
{
    public void Foo()
    {
        string c = null;
        if (c is null)
        {
            doTheThing(c);
        }
        if (c is null) // Noncompliant {{This condition was just checked on line 6.}}
        {
            doTheThing(c);
        }
        if (c is not null) // Compliant
        {
            doTheThing(c);
        }

        if (c is "banana") // Compliant, c might be updated in the previous if
        {
            c += "a";
        }
        if (c is "banana") // Compliant, c might be updated in the previous if
        {
            c = "";
        }
        if (c is "banana") // Compliant, c might be updated in the previous if
        {
            c = "";
        }

        int i = 0;
        if (i is > 0 and < 100)
        {
            doTheThing(i);
        }
        if (i is > 0 and < 100) // Noncompliant {{This condition was just checked on line 33.}}
        {
            doTheThing(i);
        }

        Fruit f = null;
        bool cond = false;
        if (f is Apple)
        {
            f = cond ? new Apple() : new Orange();
        }
        if (f is Apple) // Compliant as f may change
        {
            f = cond ? new Apple() : new Orange();
        }

        if (f is null) { }
        if (f is null) { }  // Noncompliant

        char ch = 'x';
        if (ch is >= 'a' and <= 'z') { }
        if (ch is >= 'a' and <= 'z') { } // Noncompliant
        if (ch is <= 'z' and >= 'a') { } // FN, rule is syntax based. Only null check equivalence is currenty supported in CSharpEquivalenceChecker

        if (ch is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '.' or '?') { }
        if (ch is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '.' or '?') { } // Noncompliant
        if (ch is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '?' or '.') { } // FN, rule is syntax based. Only null check equivalence is currenty supported in CSharpEquivalenceChecker
        if (ch is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '?' or '.') { } // FN, rule is syntax based. Only null check equivalence is currenty supported in CSharpEquivalenceChecker

        if (f is not null) { }
        if (f is not not not null) { }  // Noncompliant, same meaning
        if (f != null) { }              // Noncompliant, same meaning

        if (f is null) { }
        if (f == null) { }          // Noncompliant, same meaning
        if (!(f is not null)) { }   // Noncompliant, same meaning

        if (!(f is null)) { }
        if (f != null) { }          // Noncompliant, same meaning
        if (f is not not null) { }  // Compliant, equal to f == null

        if (f is Apple) { }
        if (f is Apple) { }   // Noncompliant

        if (f is Apple or Orange) { }
        if (f is Apple or Orange) { }   // Noncompliant
        if (f is Orange or Apple) { }   // FN, rule is syntax based. Only null check equivalence is currenty supported in CSharpEquivalenceChecker

        if (f is not Apple) { }
        if (f is not Apple) { } // Noncompliant

        if (f is { Size: >= 5, Value: 0 }) { }
        if (f is { Size: >= 5, Value: 0 }) { } // Noncompliant
        if (f is { Value: 0, Size: >= 5 }) { } // FN, rule is syntax based. Only null check equivalence is currenty supported in CSharpEquivalenceChecker

        if (f is { Size: >= 5 }) { }
        if (f is not { Size: < 5 }) { } // FN, rule is syntax based. Only null check equivalence is currenty supported in CSharpEquivalenceChecker
    }

    void doTheThing(object o) { }

    public void PositionalPattern(Apple a)
    {
        if (a is ("Sweet", "Red"))
        {
        }
        if (a is ("Sweet", "Red")) // Noncompliant
        {
        }
        if (a is { Taste: "Sweet", Color: "Red" }) // FN, rule is syntax based. Only null check equivalence is currenty supported in CSharpEquivalenceChecker
        {
        }
    }
}

abstract class Fruit
{
    public int Size { get; }
    public int Value { get; }
}

class Apple : Fruit
{
    public string Taste;
    public string Color;
    public void Deconstruct(out string x, out string y) => (x, y) = (Taste, Color);
}

class Orange : Fruit { }

