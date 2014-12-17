namespace Tests.Diagnostics
{
    using System.Collections.Generic;

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
