using System.Linq;

public class Sample
{
    private Person person;

    public void Basic(string expected)
    {
        _ = person is { Name: var _ };              // Noncompliant {{Don't use this deconstruction. It's pointless.}}
        //                    ^^^^^
        _ = person is { Name: var unused };         // Noncompliant {{Don't use this deconstruction. Reference the member from the instance instead.}}
        //                    ^^^^^^^^^^
        _ = person is { Name: var usedOnce }        // Noncompliant
            && usedOnce is null;
        _ = person is { Name: var usedTwice }       // Noncompliant
            && usedTwice is null
            && usedTwice is null;
        _ = person is { Name: var usedThrice }      // Compliant, used 3 or more times
            && usedThrice is null
            && usedThrice is null
            && usedThrice is null;

        _ = person is { Address: (var city, var street) } && city.Name == street;
        //                        ^^^^^^^^              Noncompliant
        //                                  ^^^^^^^^^^  Noncompliant@-1

        // The best way is to do this, but that does not demonstrate the purpose of the rule
        _ = person is { Name: not null };
        // What should be used is this, if we imagine that "person" was a method invocation instead and we need to rename it:
        _ = MayReturnPerson() is Person { } renamedPerson
            && renamedPerson.Name == expected;

        _ = person is var renamed && renamed.Name == "Lorem";  // T0034, this is not a deconstruction
    }

    public void Nested()
    {
        _ = person is
        {
            Name: var name,                 // Noncompliant
            Address:
            {
                City:
                {
                    Country: var country,   // Noncompliant
                    Name: var city,         // Compliant, used 3 times
                    Code: var code          // Noncompliant
                }
            }
        }
            && name is not null
            && country is not null
            && city is not null
            && city is not null
            && city is not null
            && code is not null;

        _ = person is { Address.City.Name: var propertyName }   // Noncompliant
            && propertyName is not null;
    }

    public void Usages()
    {
        if (false || person is { Name: var name } || false)
        {
            name.ToString();
            _ = string.Format("{0}", name);
            _ = name[0];
        }
        if (!(person is { Name: var usedInElse }))
        {
            // Nothing to see here
        }
        else
        {
            usedInElse.ToString();
            _ = string.Format("{0}", usedInElse);
            _ = usedInElse[0];
        }
    }

    public bool PropertyNoncompliant =>
        person is { Name: var name }    // Noncompliant
        && name is null
        && name is null;

    public bool PropertyCompliant =>
        person is { Name: var name }    // Used 3 times
        && name is null
        && name is null
        && name is null;

    public bool AccessorNoncompliant
    {
        get => person is { Name: var name }    // Noncompliant
                && name is null
                && name is null;
    }

    public bool Accessor
    {
        get => person is { Name: var name }    // Used 3 times
                && name is null
                && name is null
                && name is null;
    }

    public string Lambda(Person[] list)
    {
        list.Where(x => x is { Name: var name } && name is not null);   // Noncompliant
        //                           ^^^^^^^^
        string name = null;
        return name + name + name;  // Same name, different symbol
    }

    public object MayReturnPerson() => null;
}

public record Person(string Name, Address Address);

public record Address(City City, string Street);

public record City(string Country, string Name, string Code);
