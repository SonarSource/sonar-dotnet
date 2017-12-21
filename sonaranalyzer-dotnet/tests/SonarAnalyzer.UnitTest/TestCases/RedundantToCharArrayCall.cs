using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class RedundantToCharArrayCall
    {
        public char[] ToCharArray()
        {
            return null;
        }

        public void CreateNew2(int propertyValue)
        {
            var c = "some string".ToCharArray()[10]; // Noncompliant, the indexer already returns a char
//                                ^^^^^^^^^^^
            c = "some string".ToCharArray(5, 4)[1];
            foreach (var v in "some string".ToCharArray()) // Noncompliant {{Remove this redundant 'ToCharArray' call.}}
            {
                // ...
            }

            var x = "some string".ToCharArray();

            c = this.ToCharArray()[10];
        }
    }
}
