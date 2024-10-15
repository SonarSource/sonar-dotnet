class CSharp13
{
    bool StartsWithString(string str)
    {
        return str.StartsWith("\e"); //Noncompliant
    }
    bool StartsWithChar(string str)
    {
        return str.StartsWith('\e'); //Compliant
    }

    bool EndsWithString(string str)
    {
        return str.EndsWith("\e"); //Noncompliant
    }
    bool EndsWithChar(string str)
    {
        return str.EndsWith('\e'); //Compliant
    }
}
