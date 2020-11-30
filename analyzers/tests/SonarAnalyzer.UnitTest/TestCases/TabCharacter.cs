using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class TabCharacter
    {
        public TabCharacter()
        {
            var tabs = "	"; // Noncompliant
            var tabs2 = "		";
            // some more tabs: "		"
        }
    }
}
