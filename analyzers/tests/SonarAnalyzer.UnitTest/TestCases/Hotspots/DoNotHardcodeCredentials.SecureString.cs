using System.Security;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Test()
        {
            using (SecureString securePwd = new SecureString())
            {
                for (int i = 0; i < "AP@ssw0rd".Length; i++)
                {
                    securePwd.AppendChar("AP@ssw0rd"[i]); // Noncompliant {{Please review this hard-coded password.}}
                }

                // Do something with securePwd
            }
        }
    }
}
