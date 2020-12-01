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
                X, //Fixed
                Y  //Fixed
            };

            var anon2 = new
            {
                X, //Fixed
                Y = "some string"
            };
        }
    }
}
