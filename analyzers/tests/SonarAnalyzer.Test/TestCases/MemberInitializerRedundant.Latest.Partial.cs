public partial class PartialConstructor
{
    public partial PartialConstructor()
    {
        id = 1;
    }
}

public partial class MixedConstructors
{
    public partial MixedConstructors(string s);
}

public partial class MultiplePartialConstructors_Noncompliant
{
    public partial MultiplePartialConstructors_Noncompliant(int x) { id = x; }
    public partial MultiplePartialConstructors_Noncompliant(string s) { id = s.Length; }
}

public partial class MultiplePartialConstructors_Compliant
{
    int id = 100;   // Compliant - not all partial constructors set id
    public partial MultiplePartialConstructors_Compliant(int x);
    public partial MultiplePartialConstructors_Compliant(string s);
}

public partial class MultiplePartialConstructors_Compliant
{
    public partial MultiplePartialConstructors_Compliant(int x) { id = x; }
    public partial MultiplePartialConstructors_Compliant(string s) { }
}
