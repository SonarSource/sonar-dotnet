using System;

record RecordToStringNoNull
{
    public override string ToString()
    {
        if (DateTime.Now.Hour > 5)
        {
            return null; // Noncompliant
        }
        else
        {
            // ...
        }

        return null; // Noncompliant {{Return empty string instead.}}
    }
}

record RecordToStringNoNullCompliant
{
    public override string ToString()
    {
        if (DateTime.Now.Hour > 5)
        {
            return string.Empty;
        }
        else
        {
            // ...
        }

        return "";
    }
}

struct StructToStringNotNull
{
    public override string ToString()
    {
        return null; // Noncompliant
    }
}

struct StructToStringNotNullCompliant
{
    public override string ToString()
    {
        return string.Empty;
    }
}
