using System.Security.Cryptography;
using System.Text;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Hash()
        {
            var passwordBytes = Encoding.UTF8.GetBytes("password");
            var shortSalt = new byte[31];

            var pdb1 = new PasswordDeriveBytes(passwordBytes, shortSalt); // Noncompliant
        }
    }
}
