using System;
using System.Security;

namespace Tests.Diagnostics
{
    class Issue_7323
    {
        // https://github.com/SonarSource/sonar-dotnet/issues/7323
        public void Noncompliant()
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

        public void Compliant(String password)
        {
            using (SecureString securePwd = new SecureString())
            {
                for (int i = 0; i < password.Length; i++)
                {
                    securePwd.AppendChar(password[i]); // Compliant
                }

                // Do something with securePwd
            }
        }
    }

    class Tests
    {
        public void Characters()
        {
            using (var securePwd = new SecureString())
            {
                securePwd.AppendChar('P'); // Noncompliant
                securePwd.AppendChar('a'); // Noncompliant
                securePwd.AppendChar('s'); // Noncompliant
                securePwd.AppendChar('s'); // Noncompliant
                var w = 'w';
                securePwd.AppendChar(w);   // FN
            }
        }

        public void Constant()
        {
            const string keyword = "AP@ssw0rd";
            using (SecureString securePwd = new SecureString())
            {
                for (int i = 0; i < keyword.Length; i++)
                {
                    securePwd.AppendChar(keyword[i]); // Noncompliant
                }

                // Do something with securePwd
            }
        }

        public void ForEachOverString()
        {
            using (SecureString securePwd = new SecureString())
            {
                foreach (var c in "AP@ssw0rd")
                {
                    securePwd.AppendChar(c); // Noncompliant
                }

                // Do something with securePwd
            }
        }

        public void FromUnmodifiedVariable_ForLoop()
        {
            var keyword = "AP@ssw0rd";
            using (SecureString securePwd = new SecureString())
            {
                for (int i = 0; i < keyword.Length; i++)
                {
                    securePwd.AppendChar(keyword[i]); // Noncompliant
                }
            }
        }

        public void FromUnmodifiedVariable_ForEachLoop()
        {
            var keyword = "AP@ssw0rd";
            using (SecureString securePwd = new SecureString())
            {
                foreach(var c in keyword)
                {
                    securePwd.AppendChar(c); // Noncompliant
                }
            }
        }

        public void FromBytes()
        {
            var bytes = new byte[] { 80, 97, 115, 115, 119, 111, 114, 100 };
            using (SecureString securePwd = new SecureString())
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    securePwd.AppendChar(Convert.ToChar(bytes[i])); // FN.
                }
            }
        }

        public void FromCharArray()
        {
            var chars = new char[] { 'P', 'a', 's', 's', 'w', 'o', 'r', 'd' };
            using (SecureString securePwd = new SecureString())
            {
                for (int i = 0; i < chars.Length; i++)
                {
                    securePwd.AppendChar(chars[i]); // FN.
                }
            }
        }
    }

    class AppendCharFromOtherType
    {
        void AppendChar(char c) { }

        void Test()
        {
            var other = new AppendCharFromOtherType();
            const string keyword = "AP@ssw0rd";
            for (int i = 0; i < keyword.Length; i++)
            {
                other.AppendChar(keyword[i]); // Compliant. Not SecureString.AppendChar
            }
        }
    }
}
