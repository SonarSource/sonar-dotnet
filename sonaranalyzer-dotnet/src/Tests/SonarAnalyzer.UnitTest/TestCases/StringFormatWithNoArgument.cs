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
            var s = string.Format("some text"); //Noncompliant
//                  ^^^^^^^^^^^^^
            s = string.Format(
                string.Format("some text"));    //Noncompliant {{Remove this formatting call and simply use the input string.}}
            s =    string.Format(
                string.Format("{0}", 1));

            s = string.Format("{0}", 1);
            s = string.Format("{0}");
            s = string.Format(CultureInfo.InvariantCulture, "{0}", 1);
            s = string.Format(CultureInfo.InvariantCulture, "some text"); //Noncompliant
            s = string.Format(format: "some text"); //Noncompliant
            s = string.Format(format: "{0}", arg0: 1);
        }
    }
}
