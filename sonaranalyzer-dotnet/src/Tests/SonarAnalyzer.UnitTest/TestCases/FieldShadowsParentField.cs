using System;

namespace Tests.TestCases
{
    public class Fruit
    {
        protected int ripe;
        protected int flesh;

        // ...
        private int flesh_color;
    }

    public class Raspberry : Fruit
    {
        private bool ripe;  // Noncompliant {{'ripe' is the name of a field in 'Fruit'.}}
//                   ^^^^
        private static int FLESH; // Noncompliant {{'FLESH' differs only by case from 'flesh' in 'Fruit'.}}
        private static int FLESH_COLOR;
    }
}
