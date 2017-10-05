using System;

namespace Tests.TestCases
{
    public class Fruit
    {
        protected int ripe;
        protected static int leafs;
    }

    public class Raspberry : Fruit
    {
        private bool ripe;  // Noncompliant {{'ripe' is the name of a field in 'Fruit'.}}
//                   ^^^^
        protected static int leafs; // Compliant, static is ignored
    }
}
