using System;

[assembly: CLSCompliant(true)]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
namespace IntentionalFindings
{
    public static class S2479
    {
        public static readonly string VALUE = "foo​bar"; // this string contains a \u200B character
    }
}
