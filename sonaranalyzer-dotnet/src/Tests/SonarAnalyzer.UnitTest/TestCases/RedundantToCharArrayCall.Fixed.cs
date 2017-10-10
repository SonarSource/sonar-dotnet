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
            var c = "some string"[10]; // Fixed
            c = "some string".ToCharArray(5, 4)[1];
            foreach (var v in "some string") // Fixed
            {
                // ...
            }

            var x = "some string".ToCharArray();

            c = this.ToCharArray()[10];
        }
    }
}
