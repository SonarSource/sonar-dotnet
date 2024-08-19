namespace Tests.Diagnostics
{
    public class StringConcatenationInLoop
    {
        public StringConcatenationInLoop()
        {
            string s = "";
            for (int i = 0; i < 50; i++)
            {
                var sLoop = "";

                s = s + $"""{5}a""" + """b""";  // Noncompliant
                s += """a""";     // Noncompliant {{Use a StringBuilder instead.}}
                sLoop += """a"""; // Compliant

                i += 5;
            }
        }
    }
}
