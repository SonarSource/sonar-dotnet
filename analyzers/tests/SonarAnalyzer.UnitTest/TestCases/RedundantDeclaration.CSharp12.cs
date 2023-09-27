using System;
using System.Collections.Generic;

namespace AliasAnyType
{
    using Int = int;
    using IntArray = int[];
    using IntNullable = int?;
    using Point = (int x, int y);
    using ThreeInts = int[3];                   // Error [CS0270]: Array size cannot be specified in a variable declaration
    using IntToIntFunc = Func<int, int>;

    class AClass
    {
        void AliasWithArraySize()
        {
            IntArray a1 = new Int[3] { 1, 2, 3 };  // Noncompliant
        }

        void AliasWithUnnecessaryType()
        {
            IntToIntFunc f = (Int i) => i;      // Noncompliant {{Remove the type specification; it is redundant.}}
        }

        void AliasWithInitializer()
        {
            IntArray a1 = new IntArray { };     // Error [CS8386]: Invalid object creation
            var a2 = new IntArray { };          // Error [CS8386]: Invalid object creation
            int[] a3 = new IntArray();          // Error [CS8386]: Invalid object creation
        }

        void AliasWithEmptyParamsList()
        {
            IntArray a1 = new IntArray();       // Error [CS8386]: Invalid object creation
            var a2 = new IntArray();            // Error [CS8386]: Invalid object creation
            int[] a3 = new IntArray();          // Error [CS8386]: Invalid object creation
        }
    }
}
