using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Tests.Diagnostics
{
    public class StringFormatWithNoArgument
    {
        void Test()
        {
            var s = "some text"; //Fixed
            s = "some text";    //Fixed
            s =    string.Format(
                string.Format("{0}", 1));

            s = string.Format("{0}", 1);
            s = string.Format("{0}");
            s = string.Format(CultureInfo.InvariantCulture, "{0}", 1);
            s = "some text"; //Fixed
            s = "some text"; //Fixed
            s = string.Format(format: "{0}", arg0: 1);
        }
    }
}
