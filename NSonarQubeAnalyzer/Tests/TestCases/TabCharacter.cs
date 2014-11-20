using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class RightCurlyBraceStartsLine
    {
        public RightCurlyBraceStartsLine()
        {
            var tabs = "	"; // Noncompliant
            var tabs2 = "		";
            // some more tabs: "		"
        }
    }
}
