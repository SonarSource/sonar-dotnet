using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public static class RedundantArgument
    {
        public static void M(int x, int y = 5, int z = 7) { /* ... */ }
        public static void M2(int x, int y, int z) { /* ... */ }
        public static void M3(int x = 1, int y = 5, int z = 7) { /* ... */ }
        public static void Ext(this int self, int y = 5, params int[] parameters) { }
        public static void Ext2(this int self, int y, params int[] parameters) { }

        public static void Test()
        {
            var x = "".ToString();
            M(1); //Fixed
            M(1); //Fixed
            M(1); // Fixed
            M(1);
            M(1, 2, 4);
            M2(1, 1, 1);
            5.Ext(); //Fixed
            5.Ext2(5);

            RedundantArgument.Ext(5, 5, 4, 4, 5, 6); //Fixed
            RedundantArgument.Ext(5, parameters: new int[] { 4, 4, 5, 6 }); //Fixed
            RedundantArgument.Ext(5); //Fixed

            M3();//Fixed

            M3(y: 4); //Fixed
            M3(y: 4); //Fixed

            M3(1, 4); //Fixed
        }
    }
}
