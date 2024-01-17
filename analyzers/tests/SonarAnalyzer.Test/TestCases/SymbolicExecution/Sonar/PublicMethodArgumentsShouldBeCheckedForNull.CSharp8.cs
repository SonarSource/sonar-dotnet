public class NullCoalescenceAssignment
{
    public void NullCoalescenceAssignment_NotNull(string s1)
    {
        s1 ??= "N/A";
        s1.ToString(); // Compliant
    }

    public void NullCoalescenceAssignment_Null(string s1)
    {
        s1 ??= null;
        s1.ToString(); // Noncompliant
    }

    public void InsideIf(string str)
    {
        if (str != null)
        {
            str ??= null;
            str.ToString(); // Compliant, we know str is not null
        }

        if (str == null)
        {
            str ??= "foo";
            str.ToString(); // Compliant, assigned foo
        }
    }
}

public interface IWithDefaultMembers
{
    decimal Count { get; set; }
    decimal Price { get; set; }

    void Reset(string s)
    {
        s.ToString(); // Noncompliant
    }
}

public class LocalStaticFunctions
{
    public void Method(object arg)
    {
        string LocalFunction(object o)
        {
            return o.ToString(); // Compliant - FN: local functions are not supported by the CFG
        }

        static string LocalStaticFunction(object o)
        {
            return o.ToString(); // Compliant - FN: local functions are not supported by the CFG
        }
    }
}

public class Address
{
    public string Name { get; }

    public string State { get; }

    public void Deconstruct(out string name, out string state) =>
        (name, state) = (Name, State);
}

public class Person
{
    public string Name { get; }

    public Address Address { get; }

    public void Deconstruct(out string name, out Address address) =>
        (name, address) = (Name, Address);
}

public class SwitchExpressions
{
    public void OnlyDiscardBranch_Noncompliant(string s, bool b)
    {
        var result = b switch
        {
            _ => s.ToString() // Noncompliant
        };
    }

    public void MultipleBranches_Noncompliant(string s, int val)
    {
        var result = val switch
        {
            1 => "a",
            2 => s.ToString(), // Noncompliant
            _ => "b"
        };
    }

    public void Nested_Noncompliant(string s, int val, bool condition)
    {
        var result = val switch
        {
            1 => "a",
            2 => condition switch
            {
                _ => s.ToString() // Noncompliant
            },
            _ => "b"
        };
    }

    public void MultipleBranches_HandleNull(string s, int val)
    {
        var result = s switch
        {
            null => s.ToString(), // Noncompliant
            _ => s.ToString() // Compliant as null was already handled
        };
    }

    public void MultipleBranches_Compliant(string s, int val)
    {
        var result = val switch
        {
            1 => "a",
            2 => s == null ? string.Empty : s.ToString(),
            _ => "b"
        };
    }

    public string MultipleBranches_PropertyPattern(Address address, string s)
    {
        return address switch
        {
            { State: "WA" } addr => s.ToString(), // Noncompliant
            _ => string.Empty
        };
    }

    public string MultipleBranches_PropertyPattern_FP(string s)
    {
        return s switch
        {
            { Length: 5 } => s.ToString(), // Noncompliant - FP we know that the length is 5 so the string cannot be null
            _ => string.Empty
        };
    }

    public string MultipleBranches_RecursivePattern(Person person, string s)
    {
        return person switch
        {
            { Address: { State: "WA" } } pers => s.ToString(), // Noncompliant
            _ => string.Empty
        };
    }

    public string MultipleBranches_TuplePattern(Address address, string s)
    {
        return address switch
        {
            var (name, state) => s.ToString(), // Compliant - FN
            _ => string.Empty
        };
    }

    public string MultipleBranches_WhenClause(Address address, string s)
    {
        return address switch
        {
            Address addr when addr.Name.Length > 0 => s.ToString(), // Noncompliant
            Address addr when addr.Name.Length == s.Length => string.Empty, // Noncompliant
            _ => string.Empty
        };
    }

    public string MultipleBranches_VarDeclaration(Address address, string s)
    {
        return address switch
        {
            Address addr => s.ToString(), // Noncompliant
            _ => string.Empty
        };
    }

    public string TwoBranches_NoDefault(bool condition, string s)
    {
        return condition switch
        {
            true => s.ToString(), // Noncompliant
            false => s.ToString() // Noncompliant
        };
    }
}

public class SwitchStatement
{
    public void Test(string s)
    {
        switch (s)
        {
            case null:
                break;

            default:
                s.ToString(); // Compliant - the null is handled by the case null branch.
                break;
        }
    }
}
