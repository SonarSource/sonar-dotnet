class Issues
{
    bool StartsWith(string str)
    {
        return str.StartsWith('a');
    }
    bool EndsWith(string str)
    {
        return str.EndsWith('z');
    }
    bool SwapEscape(string str)
    {
        return str.EndsWith('\'')
            || str.EndsWith('"')
            || str.EndsWith('"');
    }
    bool Escaped(string str)
    {
        return str.EndsWith('\t');
    }

    void Chained(string str)
    {
        var first = str.EndsWith('a').ToString().ToString();
        var middle = str.Substring(1).EndsWith('a').ToString();
        var end = str.Substring(1).Substring(1).EndsWith('a');

        var nullConditionalOperator = str.Substring(1)?.EndsWith('a').ToString();
    }
}

class Unchanged
{
    public bool EndsWith(string str)
    {
        return str.EndsWith("12")
            || "ABC".EndsWith(str);
    }
}
