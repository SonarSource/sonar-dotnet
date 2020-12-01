using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class TernaryOperatorPointless
    {
        public TernaryOperatorPointless(bool b  )
        {
            var x = 1> 18 ? true : true; // Noncompliant {{This operation returns the same value whether the condition is 'true' or 'false'.}}
//                  ^^^^^^^^^^^^^^^^^^^
            var y = 1> 18 ? true : false;
            y = 1 > 18 ? (true) : true; //Noncompliant
            new TernaryOperatorPointless(1 > 18 ? (true) : true);//Noncompliant
        }
    }
}
