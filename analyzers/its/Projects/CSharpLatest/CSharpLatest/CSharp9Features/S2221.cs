﻿using System;
using System.IO;

namespace CSharpLatest.CSharp9Features;

public class S2221
{
    public void Foo()
    {
        try { }
        catch (Exception e) when (e is not FileNotFoundException) { }
    }
}
