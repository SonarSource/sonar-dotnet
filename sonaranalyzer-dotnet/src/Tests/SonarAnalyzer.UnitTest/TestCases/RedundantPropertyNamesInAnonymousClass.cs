using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class RedundantPropertyNamesInAnonymousClass
    {
        public int X { get; set; }

        public void M()
        {
            var Y = "my string";

            var anon = new
            {
                X = X, //Noncompliant
//              ^^^
                Y = Y  //Noncompliant {{Remove the redundant 'Y ='.}}
            };

            var anon2 = new
            {
                X = X, //Noncompliant
                Y = "some string"
            };
        }
    }
}
