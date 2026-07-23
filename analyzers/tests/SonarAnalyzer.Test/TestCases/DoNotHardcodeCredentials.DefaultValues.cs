using System;
using System.Net;
using System.Security;
using System.Security.Cryptography;

namespace Tests.Diagnostics
{
    class Program
    {
        public const string DBConnectionString = "Server=localhost; Database=Test; User=SA; Password=aabbCC123xxYY";    // Noncompliant {{"password" detected here, make sure this is not a hard-coded credential.}}
        public const string EditPasswordPageUrlToken = "{Account.EditPassword.PageUrl}"; // Compliant

        private const string secretConst = "constantValue";
        private string secretField = "literalValue";
        private string secretFieldConst = secretConst;
        private string secretFieldUninitialized;
        private string secretFieldNull = null;
        private string secretFieldMethod = SomeMethod();
        private string invalidField = invalidField; // // Error [CS0236] A field initializer cannot reference the non-static field, method, or property

        private static string SomeMethod() => "";

        public void Test(string user)
        {
            string password = @"foo"; // Noncompliant {{"password" detected here, make sure this is not a hard-coded credential.}}
//                 ^^^^^^^^^^^^^^^^^

            string foo, passwd = "a"; // Noncompliant {{"passwd" detected here, make sure this is not a hard-coded credential.}}
//                      ^^^^^^^^^^^^

            string pwdPassword = "a"; // Noncompliant {{"pwd and password" detected here, make sure this is not a hard-coded credential.}}

            string foo2 = @"Password=$uper$trongPa$$word123@#"; // Noncompliant
            string multiline = // Noncompliant
                @"Server=A;
                User=B;
                Password=ToBe|~2Be123";

            string multiline_OK =
                @"Password detected here,
                make sure this is not
                a hard-coded credential.";

            string bar;
            bar = "Password=aabbCC123xyZZ"; // Noncompliant

            object obj;
            obj = "Password=p"; // Compliant, only assignment to string is inspected

            foo = "password";
            foo = "password=";
            foo = "passwordpassword";
            foo = "foo=1;password=S3crEt@123";  // Noncompliant
            foo = "foo=1password=S3crEt@123";   // Noncompliant {{"password" detected here, make sure this is not a hard-coded credential.}}
            foo = "";                           // Compliant
            foo = "userpassword=S3crEt@123";    // Noncompliant {{"password" detected here, make sure this is not a hard-coded credential.}}
            foo = "passwordfield=1";            // Compliant
            foo = "user_password=S3crEt@123";   // Noncompliant
            foo = "user-password=S3crEt@123";   // Noncompliant
            foo = "user/password=S3crEt@123";   // Noncompliant
            foo = "password:1";                 // Compliant

            var something1 = (foo = "foo") + (bar = "bar");
            var something2 = (foo = "foo") + (bar = "password=S3crEt@123!"); // Noncompliant
            var something3 = (foo = "foo") + (bar = "password");
            var something4 = (foo = "foo") + (bar = "123=password");

            string myPassword1 = null;
            string myPassword2 = "";
            string myPassword3 = "        ";
            string myPassword4 = @"foo";                                // Noncompliant
            string query2 = "password=hardcoded;user='" + user + "'";   // Noncompliant
        }

        public void DefaultKeywords()
        {
            string password = "a";                  // Noncompliant
            string x1 = "password=S3crEt@123";      // Noncompliant

            string passwd = "a";                    // Noncompliant
            string x2 = "passwd=S3crEt@123";        // Noncompliant

            string pwd = "a";                       // Noncompliant
            string x3 = "pwd=S3crEt@123";           // Noncompliant

            string passphrase = "a";                // Noncompliant
            string x4 = "passphrase=S3crEt@123";    // Noncompliant
        }

        public void Constants()
        {
            const string ConnectionString = "Server=localhost; Database=Test; User=SA; Password=S3crEt@123";                     // Noncompliant
            const string ConnectionStringWithSpaces = "Server=localhost; Database=Test; User=SA; Password   =   S3crEt@123";     // Noncompliant
            const string inTheMiddle = "Server=localhost; Database=Test; User=SA; Password=S3crEt@123; Application Name=Sonar";  // Noncompliant
            const string withSemicolon = @"Server=localhost; Database=Test; User=SA; Password=""S3crEt;'123""";                  // Noncompliant
            const string withApostroph = @"Server=localhost; Database=Test; User=SA; Password='S3crEt""123";                     // Noncompliant
            const string Password = "Secret123";  // Noncompliant

            const string LoginName = "Admin";
            const string Localhost = "localhost";
        }

        public void Concatenations(string arg)
        {
            var secretVariable = "literalValue";
            var secretVariableConst = secretConst;
            string secretVariableNull = null;
            var secretVariableMethod = SomeMethod();
            string a;

            a = "Server = localhost; Database = Test; User = SA; Password = " + "hardcoded";        // Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " + secretConst;        // Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " + secretField;        // Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " + secretFieldConst;   // Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " + secretVariable;     // Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " + secretVariableConst;// Noncompliant

            a = "Server = localhost; Database = Test; User = SA; Password = " + secretFieldUninitialized;   // Compliant, not initialized to constant
            a = "Server = localhost; Database = Test; User = SA; Password = " + secretFieldNull;            // Compliant, not initialized to constant
            a = "Server = localhost; Database = Test; User = SA; Password = " + secretVariableNull;         // Compliant, not initialized to constant
            a = "Server = localhost; Database = Test; User = SA; Password = " + secretVariableMethod;       // Compliant, not initialized to constant
            a = "Server = localhost; Database = Test; User = SA; Password = " + arg;                        // Compliant, not initialized to constant

            const string passwordPrefixConst = "Password = ";       // Compliant by its name
            var passwordPrefixVariable = "Password = ";             // Compliant by its name
            a = "Server = localhost;" + " Database = Test; User = SA; Password = " + secretConst;                   // Noncompliant
            a = "Server = localhost;" + " Database = Test; User = SA; Pa" + "ssword = " + secretConst;              // FN, we don't track all concatenations to avoid duplications
            a = "Server = localhost;" + " Database = Test; User = SA; " + passwordPrefixConst + secretConst;        // Noncompliant
            a = "Server = localhost;" + " Database = Test; User = SA; " + passwordPrefixVariable + secretConst;     // Noncompliant
            a = "Server = localhost;" + " Database = Test; User = SA; Password = " + secretConst + " suffix";       // Noncompliant
            //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            a = SomeMethod() + " Database = Test; User = SA; Password = " + secretConst + " suffix";                // Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " + secretConst + arg + " suffix";      // Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " + arg + secretConst + " suffix";      // Compliant
            a = secretConst + "Server = localhost; Database = Test; User = SA; Password = " + arg;                  // Compliant
            a = "Server = localhost; Database = Test; User = SA; " + SomeMethod() + secretConst;                    // Compliant

            // Reassigned
            arg += "Literal";
            a = "Server = localhost; Database = Test; User = SA; Password = " + arg;                    // Compliant, += is not a constant propagating operation
            secretVariableMethod = "literal";
            a = "Server = localhost; Database = Test; User = SA; Password = " + secretVariableMethod;   // Noncompliant
            arg = "literal";
            a = "Server = localhost; Database = Test; User = SA; Password = " + arg;                    // Noncompliant
            secretVariable = SomeMethod();
            a = "Server = localhost; Database = Test; User = SA; Password = " + secretVariable;         // Compliant

            var invalidVariable = invalidVariable; // Error [CS0841] Cannot use local variable invalid before it's declared
            a = "Server = localhost; Database = Test; User = SA; Password = " + invalidVariable;        // Compliant, test to avoid infinite recursion
            a = "Server = localhost; Database = Test; User = SA; Password = " + invalidField;           // Compliant, test to avoid infinite recursion
        }

        public void Interpolations(string arg)
        {
            var secretVariable = "literalValue";
            string a;
            a = $"Server = localhost; Database = Test; User = SA; Password = {secretConst}";        // Noncompliant
            a = $"Server = localhost; Database = Test; User = SA; Password = {secretField}";        // Noncompliant
            a = $"Server = localhost; Database = Test; User = SA; Password = {secretVariable}";     // Noncompliant
            a = $"Server = localhost; Database = Test; User = SA; Password = {arg}";                // Compliant
            a = $"Server = localhost; Database = Test; User = SA; Password = {arg}{secretConst}";   // Compliant
            a = $@"Server = localhost; Database = Test; User = SA; Password = {secretConst}";       // Noncompliant
        }

        public void StringFormat(string arg, IFormatProvider formatProvider, string[] arr)
        {
            var secretVariable = "literalValue";
            string a;
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {0}", secretConst);           // Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {1}", null, secretConst);     // Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {1}", null, secretField);     // Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {2}", 0, 0, secretVariable);  // Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {0}", arg);                   // Compliant
            a = String.Format(@"Server = localhost; Database = Test; User = SA; Password = {0}", secretConst);          // Noncompliant
            a = String.Format(formatProvider, "Database = Test; User = SA; Password = {0}", secretConst);               // Compliant, we can't simulate formatProvider behavior
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {0}", arr);                   // Compliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {invalid}", secretConst);     // Compliant, the format is invalid and we should not raise
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = invalid {0", secretConst);    // Compliant, the format is invalid and we should not raise
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {0:#,0.00}", arg);            // Compliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {0}{1}{2}", arg);             // Compliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = hardcoded");                  // Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = {0}; Password = hardcoded", arg);            // Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {1}{0}", arg, secretConst);   // Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {0}{1}", arg, secretConst);   // Compliant
            a = String.Format("{0} Argument 1 is not used","Hello", "User = SA; Password = hardcoded");                 // Compliant

            a = String.Format(arg0: secretConst, format: "Server = localhost; Database = Test; User = SA; Password = {0}");  // FN, not supported
        }

        public void RefVariable()
        {
            var secret = "hardcoded";
            FillRef(ref secret);
            var a = "Server = localhost; Database = Test; User = SA; Password = " + secret;   // Compliant
        }

        private void FillRef(ref string arg) =>
            arg = SomeMethod();

        public void StandardAPI(SecureString secureString, string nonHardcodedPassword, byte[] byteArray, CspParameters cspParams)
        {
            const string secretLocalConst = "hardcodedSecret";
            var secretVariable = "literalValue";
            var secretVariableConst = secretConst;
            string secretVariableNull = null;
            var secretVariableMethod = SomeMethod();
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

            new NetworkCredential("username", secretConst);         // Noncompliant {{Please review this hard-coded password.}}
            new NetworkCredential("username", secretLocalConst);    // Noncompliant {{Please review this hard-coded password.}}
            new NetworkCredential("username", secretField);         // Noncompliant
            new NetworkCredential("username", secretFieldConst);    // Noncompliant
            new NetworkCredential("username", secretVariable);      // Noncompliant
            new NetworkCredential("username", secretVariableConst); // Noncompliant
            new NetworkCredential("username", "hardcoded");         // Noncompliant {{Please review this hard-coded password.}}
            new NetworkCredential("username", "hardcoded", "domain");   // Noncompliant {{Please review this hard-coded password.}}
            networkCredential.Password = "hardcoded";               // Noncompliant
            networkCredential.Password = secretVariable;            // Noncompliant
            networkCredential.Password = secretField;               // Noncompliant
            new PasswordDeriveBytes("hardcoded", byteArray);        // Noncompliant {{Please review this hard-coded password.}}
            new PasswordDeriveBytes("hardcoded", byteArray, cspParams);                     // Noncompliant {{Please review this hard-coded password.}}
            new PasswordDeriveBytes("hardcoded", byteArray, "strHashName", 1);              // Noncompliant {{Please review this hard-coded password.}}
            new PasswordDeriveBytes("hardcoded", byteArray, "strHashName", 1, cspParams);   // Noncompliant {{Please review this hard-coded password.}}

            // Compliant
            new NetworkCredential("username", secretFieldUninitialized);
            new NetworkCredential("username", secretFieldNull);
            new NetworkCredential("username", secretFieldMethod);
            new NetworkCredential("username", secretVariableNull);
            new NetworkCredential("username", secretVariableMethod);
            networkCredential.Password = secretVariableMethod;
            networkCredential.Password = secretFieldMethod;
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
            string query10 = "password=@password";        // Compliant, SQL Server named parameter
            string query11 = "password=@pwd";             // Compliant, SQL Server named parameter
            string query12 = "UPDATE table SET password = @password";     // Compliant, SQL Server named parameter
            string query13 = @"Server=myServerName\myInstanceName;Database=myDataBase;Password=@myPassword;User Id=@username;"; // Compliant, SQL Server named parameter
            string query14 = "password=$password";        // Compliant, SQLite named parameter
            string query15 = "password=$pwd";             // Compliant, SQLite named parameter
            string query16 = "UPDATE table SET password = $password";     // Compliant, SQLite named parameter
            using (var conn = OpenConn("Server = localhost; Database = Test; User = SA; Password = ?")) { }
            using (var conn = OpenConn("Server = localhost; Database = Test; User = SA; Password = :password")) { }
            using (var conn = OpenConn("Server = localhost; Database = Test; User = SA; Password = {0}")) { }
            using (var conn = OpenConn("Server = localhost; Database = Test; User = SA; Password = ")) { }
            using (var conn = OpenConn("Server = localhost; Database = Test; User = SA; Password = @password")) { } // Compliant, SQL Server named parameter
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
            var expression = (password = "Password") + (pwd = "pwd");

            string myPassword = "pwd"; // Noncompliant, different value from word list is used
        }

        public void UriWithUserInfo(string pwd, string domain)
        {
            string n1 = "scheme://user:azerty123@domain.com"; // Noncompliant {{Review this hard-coded URI, which may contain a credential.}}
            string n2 = "scheme://user:With%20%3F%20Encoded@domain.com";            // Noncompliant
            string n3 = "scheme://user:With!$&'()*+,;=OtherCharacters@domain.com";  // Noncompliant
            string n4 = "scheme://user:Pa$$word:With:Colons@domain.com";            // Noncompliant
            string n5 = "scheme://user:azerty123@" + domain;                        // Noncompliant

            string c1 = "scheme://user:" + pwd + "@domain.com";
            string c2 = "scheme://user:@domain.com";
            string c3 = "scheme://user@domain.com:80";
            string c4 = "scheme://user@domain.com";
            string c5 = "scheme://domain.com/user:azerty123";
            string c6 = String.Format("scheme://user:{0}@domain.com", pwd);
            string c7 = $"scheme://user:{pwd}@domain.com";
            string c8 = $"scheme://user:{secretConst}@domain.com";  // Noncompliant

            string e1 = "scheme://admin:admin@domain.com";      // Compliant exception, user and password are the same
            string e2 = "scheme://abc:abc@domain.com";          // Compliant exception, user and password are the same
            string e3 = "scheme://a%20;c:a%20;c@domain.com";    // Compliant exception, user and password are the same
            string e4 = "scheme://user:;@domain.com";           // Compliant exception for implementation purposes

            string html1 = // Noncompliant
@"This is article http://login:azerty123@www.example.com
Email: info@example.com
Phone: +0000000";

            string html2 =
@"This is article http://www.example.com
Email: info@example.com
Phone: +0000000";

            string html3 = "This is article http://www.example.com Email: info@example.com Phone: +0000000";
            string html4 = "This is article http://www.example.com<br>Email:info@example.com<br>Phone:+0000000";
            string html5 = "This is article http://user:S3cRet123@www.example.com<br>Email:info@example.com<br>Phone:+0000000"; // Noncompliant
        }

        public void LiteralAsArgument(string pwd, string server)
        {
            using (var conn = new SqlConnection("Server = localhost; Database = Test; User = SA; Password = S3cRet123")) { } // Noncompliant
            using (var conn = OpenConn("Server = localhost; Database = Test; User = SA; Password = S3cRet123")) { }          // Noncompliant
            using (var conn = OpenConn("Server = " + server + "; Database = Test; User = SA; Password = S3cRet123")) { }     // Noncompliant

            using (var conn = OpenConn("password")) { }
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
                return "Server = localhost; Database = Test; User = SA; Password = S3CrEt123"; // Noncompliant
            }
        }

        public string SecretConnectionStringProperty_OK
        {
            get
            {
                return "Nothing to see here";
            }
        }

        public string SecretConnectionStringProperty2 => "Server = localhost; Database = Test; User = SA; Password = S3CrEt123"; // Noncompliant
        public string SecretConnectionStringProperty2_OK => "Nothing to see here";

        public string SecretConnectionStringProperty3 { get; } = "Server = localhost; Database = Test; User = SA; Password = S3CreT123"; // Noncompliant
        public string SecretConnectionStringProperty3_OK { get; } = "Nothing to see here";

        public string SecretConnectionStringFunction()
        {
            return "Server = localhost; Database = Test; User = SA; Password = S3cRet456"; // Noncompliant
        }

        public string SecretConnectionStringFunction_OK()
        {
            return "Nothing to see here";
        }

        public string SecretConnectionStringFunction2() => "Server = localhost; Database = Test; User = SA; Password = Seeree123"; // Noncompliant
        public string SecretConnectionStringFunction2_OK() => "Nothing to see here";

        public string SecretConnectionStringFunction3() => @"Server = localhost; Database = Test; User = SA; Password = Sssree123"; // Noncompliant
        public string SecretConnectionStringFunction3_OK() => @"Nothing to see here";
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
            string query1 = "password=':cr_zy1;s3cret2';user=xxx"; // Noncompliant
        }

        class Configuration
        {
            public static string Password { get; set; }
        }
    }
}
