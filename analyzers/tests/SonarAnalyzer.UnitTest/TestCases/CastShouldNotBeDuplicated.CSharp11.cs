using System;
using System.Collections.Generic;

void ListPattern()
{
    object[] numbers = { 1, 2, 3 };

    if (numbers is [double something, 3, 3])    // FN
    {
        var ff1 = (double)something;            // FN
    }
}
