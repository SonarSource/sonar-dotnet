using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class TernaryOperatorPointless
    {
        public TernaryOperatorPointless(bool b  )
        {
            var x = true; // Fixed
            var y = 1> 18 ? true : false;
            y = true; //Fixed
            new TernaryOperatorPointless(true);//Fixed
        }
    }
}
