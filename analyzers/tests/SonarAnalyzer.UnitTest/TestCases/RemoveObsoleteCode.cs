using System;

namespace Tests
{
    [Obsolete] // Noncompliant {{Do not forget to remove this deprecated code someday.}}
//   ^^^^^^^^
    public class Program
    {
        [Obsolete("Message")]                // Noncompliant
        public delegate void CloseDelegate(object sender, EventArgs eventArgs);

        [Obsolete("Message", error: true)]   // Noncompliant
        public event CloseDelegate OnClose;

        [ObsoleteAttribute()]                // Noncompliant
        public Program()
        {

        }
    }
}
