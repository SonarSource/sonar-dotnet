using System;
using System.Net;
using System.Security;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    class Program
    {
        public const string DBConnectionString = "Server=localhost; Database=Test; User=SA; Password=Secret123";    // Noncompliant

        private const string secret = "constantValue";

        public void Test(string user)
        {
            string password = @"foo"; // Noncompliant {{"password" detected here, make sure this is not a hard-coded credential.}}
//                 ^^^^^^^^^^^^^^^^^

            string foo, passwd = "a"; // Noncompliant {{"passwd" detected here, make sure this is not a hard-coded credential.}}
//                      ^^^^^^^^^^^^

            string pwdPassword = "a"; // Noncompliant {{"pwd, password" detected here, make sure this is not a hard-coded credential.}}

            string foo2 = @"Password=123"; // Noncompliant

            string bar;
            bar = "Password=p"; // Noncompliant
//          ^^^^^^^^^^^^^^^^^^

            foo = "password=";
            foo = "passwordpassword";
            foo = "foo=1;password=1"; // Noncompliant
            foo = "foo=1password=1";
            foo = ""; // Compliant

            string myPassword1 = null;
            string myPassword2 = "";
            string myPassword3 = "        ";
            string myPassword4 = @"foo"; // Noncompliant
            string query2 = "password=hardcoded;user='" + user + "'"; // Noncompliant
        }

        public void DefaultKeywords()
        {
            string password = "a";       // Noncompliant
            string x1 = "password=a";    // Noncompliant

            string passwd = "a";         // Noncompliant
            string x2 = "passwd=a";      // Noncompliant

            string pwd = "a";            // Noncompliant
            string x3 = "pwd=a";         // Noncompliant

            string passphrase = "a";     // Noncompliant
            string x4 = "passphrase=a";  // Noncompliant
        }

        public void Constants()
        {
            const string ConnectionString = "Server=localhost; Database=Test; User=SA; Password=Secret123";    // Noncompliant
            const string ConnectionStringWithSpaces = "Server=localhost; Database=Test; User=SA; Password   =   Secret123";    // Noncompliant
            const string Password = "Secret123";  // Noncompliant

            const string LoginName = "Admin";
            const string Localhost = "localhost";
        }

        public void StandardAPI(SecureString secureString, string nonHardcodedPassword, byte[] byteArray, CspParameters cspParams)
        {
            var networkCredential = new NetworkCredential();
            networkCredential.Password = nonHardcodedPassword;
            networkCredential.Domain = "hardcodedDomain";
            new NetworkCredential("username", secureString);
            new NetworkCredential("username", nonHardcodedPassword);
            new NetworkCredential("username", secureString, "domain");
            new NetworkCredential("username", nonHardcodedPassword, "domain");

            new PasswordDeriveBytes(nonHardcodedPassword, byteArray);
            new PasswordDeriveBytes(new byte[] { 1 }, byteArray);
            new PasswordDeriveBytes(nonHardcodedPassword, byteArray, cspParams);
            new PasswordDeriveBytes(new byte[] { 1 }, byteArray, cspParams);
            new PasswordDeriveBytes(nonHardcodedPassword, byteArray, "strHashName", 1);
            new PasswordDeriveBytes(new byte[] { 1 }, byteArray, "strHashName", 1);
            new PasswordDeriveBytes(nonHardcodedPassword, byteArray, "strHashName", 1, cspParams);
            new PasswordDeriveBytes(new byte[] { 1 }, byteArray, "strHashName", 1, cspParams);

            new NetworkCredential("username", secret); // Noncompliant
            new NetworkCredential("username", "hardcoded"); // Noncompliant
            new NetworkCredential("username", "hardcoded", "domain"); // Noncompliant
            networkCredential.Password = "hardcoded"; // Noncompliant
            new PasswordDeriveBytes("hardcoded", byteArray); // Noncompliant
            new PasswordDeriveBytes("hardcoded", byteArray, cspParams); // Noncompliant
            new PasswordDeriveBytes("hardcoded", byteArray, "strHashName", 1); // Noncompliant
            new PasswordDeriveBytes("hardcoded", byteArray, "strHashName", 1, cspParams); // Noncompliant
        }

        public void CompliantParameterUse(string pwd)
        {
            string query1 = "password=?";
            string query2 = "password=:password";
            string query3 = "password=:param";
            string query4 = "password='" + pwd + "'";
            string query5 = "password={0}";
            string query6 = "password=;user=;";
            string query7 = "password=:password;user=:user;";
            string query8 = "password=?;user=?;";
            string query9 = @"Server=myServerName\myInstanceName;Database=myDataBase;Password=:myPassword;User Id=:username;";
        }

        public void WordInVariableNameAndValue()
        {
            // It's compliant when the word is used in name AND the value.
            const string PASSWORD = "Password";
            const string Password_Input = "[id='password']";
            const string PASSWORD_PROPERTY = "custom.password";
            const string TRUSTSTORE_PASSWORD = "trustStorePassword";
            const string CONNECTION_PASSWORD = "connection.password";
            const string RESET_PASSWORD = "/users/resetUserPassword";
            const string RESET_PASSWORD_CS = "/uzivatel/resetovat-heslo"; // Noncompliant, "heslo" means "password", but we don't translate SEO friendly URL for all languages

            string passwordKey = "Password";
            string passwordProperty = "config.password.value";
            string passwordName = "UserPasswordValue";
            string password = "Password";
            string pwd = "pwd";

            string myPassword = "pwd"; // Noncompliant, different value from word list is used
        }

        public void UriWithUserInfo(string pwd, string domain)
        {
            string n1 = "scheme://user:azerty123@domain.com"; // Noncompliant {{Review this hard-coded URI, which may contain a credential.}}
            string n2 = "scheme://user:With%20%3F%20Encoded@domain.com";              // Noncompliant
            string n3 = "scheme://user:With!$&'()*+,;=OtherCharacters@domain.com";    // Noncompliant
            string n4 = "scheme://user:azerty123@" + domain;  // Noncompliant

            string c1 = "scheme://user:" + pwd + "@domain.com";
            string c2 = "scheme://user:@domain.com";
            string c3 = "scheme://user@domain.com:80";
            string c4 = "scheme://user@domain.com";
            string c5 = "scheme://domain.com/user:azerty123";
            string c6 = String.Format("scheme://user:{0}@domain.com", pwd);
            string c7 = $"scheme://user:{pwd}@domain.com";

            string e1 = "scheme://admin:admin@domain.com";    // Compliant exception, user and password are the same
            string e2 = "scheme://abc:abc@domain.com";        // Compliant exception, user and password are the same
            string e3 = "scheme://a%20;c:a%20;c@domain.com";  // Compliant exception, user and password are the same
        }

        public void LiteralAsArgument(string pwd, string server)
        {
            using (var conn = new SqlConnection("Server = localhost; Database = Test; User = SA; Password = Secret123")) { } // Noncompliant
            using (var conn = OpenConn("Server = localhost; Database = Test; User = SA; Password = Secret123")) { } // Noncompliant
            using (var conn = OpenConn("Server = " + server + "; Database = Test; User = SA; Password = Secret123")) { } // Noncompliant

            using (var conn = OpenConn("Server = localhost; Database = Test; User = SA; Password = " + pwd)) { }
        }

        private SqlConnection OpenConn(string connectionString)
        {
            var ret = new SqlConnection(connectionString);
            ret.Open();
            return ret;
        }

        public string SecretConnectionStringProperty
        {
            get
            {
                return "Server = localhost; Database = Test; User = SA; Password = Secret123"; // Noncompliant
            }
        }

        public string SecretConnectionStringProperty_OK
        {
            get
            {
                return "Nothing to see here";
            }
        }

        public string SecretConnectionStringProperty2 => "Server = localhost; Database = Test; User = SA; Password = Secret123"; // Noncompliant
        public string SecretConnectionStringProperty2_OK => "Nothing to see here";

        public string SecretConnectionStringProperty3 { get; } = "Server = localhost; Database = Test; User = SA; Password = Secret123"; // Noncompliant
        public string SecretConnectionStringProperty3_OK { get; } = "Nothing to see here";

        public string SecretConnectionStringFunction()
        {
            return "Server = localhost; Database = Test; User = SA; Password = Secret123"; // Noncompliant
        }

        public string SecretConnectionStringFunction_OK()
        {
            return "Nothing to see here";
        }

        public string SecretConnectionStringFunction2() => "Server = localhost; Database = Test; User = SA; Password = Secret123"; // Noncompliant
        public string SecretConnectionStringFunction2_OK() => "Nothing to see here";
    }

    class SqlConnection : IDisposable
    {
        public SqlConnection(string connectionString) { }
        public void Open() { }
        public void Dispose() { }
    }

    class FalseNegatives
    {
        private string password;

        public void Foo(string user)
        {
            this.password = "foo"; // False Negative
            Configuration.Password = "foo"; // False Negative
            this.password = Configuration.Password = "foo"; // False Negative
            string query1 = "password=':crazy;secret';user=xxx"; // False Negative - passwords enclosed in '' are not covered
        }

        class Configuration
        {
            public static string Password { get; set; }
        }
    }
}
