// version: CSharp9
object n = 5;
if (n != null && n is nint) // Noncompliant
{
}

if (n is not null && n is nint) // Compliant - FN
{
}

if (n is not null && n is not null) // Compliant - FN
{
}

if (n is not null && n != null) // Noncompliant
{
}

if (!(n is null) && n is nint) // Compliant
{
}

static int GetTax(object id) => id switch
{
    not null and 1 => 0, // Compliant - FN
    not null and not 5 => 5, // Compliant
    5 => 15,
    _ => 10
};
