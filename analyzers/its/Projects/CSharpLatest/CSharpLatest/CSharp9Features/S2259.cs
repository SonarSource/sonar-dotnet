﻿namespace CSharpLatest.CSharp9Features;

public class S2259
{
    private string field;

    public int PropertySimple
    {
        get => 42;
        init
        {
            object o = null;
            field = o.ToString();
        }
    }
}
