class Issues
{
    bool StartsWith(string str)
    {
        return str.StartsWith("a");
    }
    bool EndsWith(string str)
    {
        return str.EndsWith("z");
    }
    bool SwapEscape(string str)
    {
        return str.EndsWith("'")
            || str.EndsWith("\"")
            || str.EndsWith(@"""");
    }
    bool Escaped(string str)
    {
        return str.EndsWith("\t");
    }
}
