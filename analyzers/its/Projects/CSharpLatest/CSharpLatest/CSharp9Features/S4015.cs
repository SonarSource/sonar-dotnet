﻿namespace CSharpLatest.CSharp9Features;

public class S4015
{
    class A
    {
        public int Property { get; init; }
    }

    class B : A
    {
        public int Property { get; private init; }
    }
}
