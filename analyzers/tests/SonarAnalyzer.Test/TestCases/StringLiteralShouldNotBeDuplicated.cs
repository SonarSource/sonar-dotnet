namespace Tests.Diagnostics
{
    public class Program
    {
        private string empty = "";

        public const string NameConst = "foobar"; // Noncompliant {{Define a constant instead of using this literal 'foobar' 8 times.}}
        //                              ^^^^^^^^
        public static readonly string NameReadonly = "foobar";
        //                                           ^^^^^^^^ Secondary


        private string name = "foobar";
        //                    ^^^^^^^^ Secondary

        private string[] values = new[] { "something", "something", "something" }; // Compliant - repetition below threshold

        private string Name { get; } = "foobar";
        //                             ^^^^^^^^ Secondary

        public Program()
        {
            var x = "foobar";
            //      ^^^^^^^^ Secondary

            var y = "FooBar"; // Compliant - casing is different
        }

        public void Do(string s = "foobar")
        //                        ^^^^^^^^ Secondary
        {
            var x = s ?? "foobar";
            //           ^^^^^^^^ Secondary

            string GetFooBar()
            {
                return "foobar";
                //     ^^^^^^^^ Secondary
            }
        }

        public void Validate(object foobar)
        {
            if (foobar == null)
            {
                throw new System.ArgumentNullException("foobar"); // Compliant - matches one of the parameter name
            }

            Do("foobar"); // Compliant - matches one of the parameter name
        }
    }

    public class OuterClass
    {
        private string Name { get; } = "foobar"; // Noncompliant

        private class InnerClass
        {
            private string name1 = "foobar"; // Secondary - inner class count with base
            private string name2 = "foobar"; // Secondary
            private string name3 = "foobar"; // Secondary
            private string name4 = "foobar"; // Secondary
        }

        private struct InnerStruct
        {
            private string name1;
            private string name2;
            private string name3;
            private string name4;

            public InnerStruct(string s)
            {
                name1 = "foobar"; // Secondary - inner struct count with base
                name2 = "foobar"; // Secondary
                name3 = "foobar"; // Secondary
                name4 = "foobar"; // Secondary
            }
        }
    }

    public struct OuterStruct
    {
        private string Name;
        public OuterStruct(string s)
        {
            Name = "foobar"; // Noncompliant
        }

        private struct InnerStruct
        {
            private string name1;
            private string name2;
            private string name3;
            private string name4;

            public InnerStruct(string s)
            {
                name1 = "foobar"; // Secondary - inner struct count with base
                name2 = "foobar"; // Secondary
                name3 = "foobar"; // Secondary
                name4 = "foobar"; // Secondary
            }
        }
    }

    public class SpecialChar
    {
        // See https://github.com/SonarSource/sonar-dotnet/issues/2191
        private string ZZ_TRANS_PACKED_0 =
            "\x0001\u0124\x0001\x0000\x0001\u0125\x0002\x0000\x0001\u0126\x0001\x0000\x0001\u0127\x0005\x0000" + // Noncompliant {{Define a constant instead of using this literal '\x0001\u0124\x0001\x0000\x0001\u0125\x0002\x0000\x0001\u0126\x0001\x0000\x0001\u0127\x0005\x0000' 4 times.}}
            "\x0001\u0124\x0001\x0000\x0001\u0125\x0002\x0000\x0001\u0126\x0001\x0000\x0001\u0127\x0005\x0000" + // Secondary
            "\x0001\u0124\x0001\x0000\x0001\u0125\x0002\x0000\x0001\u0126\x0001\x0000\x0001\u0127\x0005\x0000" + // Secondary
            "\x0001\u0124\x0001\x0000\x0001\u0125\x0002\x0000\x0001\u0126\x0001\x0000\x0001\u0127\x0005\x0000";  // Secondary

        private string someString = @"cheese" // Noncompliant
            + "cheese"      // Secondary
            + "cheese"      // Secondary
            + @"cheese";    // Secondary

        private string backslash = "Filename\\" // Noncompliant
            + @"Filename\"                      // Secondary
            + "Filename\x005C"                  // Secondary
            + "Filename\x005C";                 // Secondary

        private string doubleQuotes = "Say \"hello\"" // Noncompliant {{Define a constant instead of using this literal 'Say \"hello\"' 4 times.}}
            + @"Say ""hello"""                        // Secondary
            + "Say \x0022hello\x0022"                 // Secondary
            + "Say \"hello\"";                        // Secondary
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9569
namespace SqlNamedParameters
{
    public class Program
    {
        public void ExecuteSqlCommands()
        {
            var userCommand = new SqlCommand("SELECT * FROM Users WHERE Name = @Name");
            userCommand.AddParameter(new SqlParameter("@Name", "John Doe"));                    // Noncompliant - FP: @Name refers to parameters in different SQL tables.
            var users = userCommand.ExecuteQuery();                                             // Renaming one does not necessitate renaming of parameters with the same name from other tables.

            var companyCommand = new SqlCommand("SELECT * FROM Companies WHERE Name = @Name");
            companyCommand.AddParameter(new SqlParameter("@Name", "Contosco"));                 // Secondary - FP
            var companies = companyCommand.ExecuteQuery();

            var productCommand = new SqlCommand("SELECT * FROM Products WHERE Name = @Name");
            productCommand.AddParameter(new SqlParameter("@Name", "CleanBot 9000"));            // Secondary - FP
            var products = productCommand.ExecuteQuery();

            var countryCommand = new SqlCommand("SELECT * FROM Countries WHERE Name = @Name");
            countryCommand.AddParameter(new SqlParameter("@Name", "Norway"));                   // Secondary - FP
            var countries = countryCommand.ExecuteQuery();
        }
    }

    public class SqlCommand
    {
        public string CommandText { get; }
        public SqlCommand(string commandText) => CommandText = commandText;
        public void AddParameter(SqlParameter parameter) { }
        public object ExecuteQuery() => null;
    }

    public class SqlParameter
    {
        public string Name { get; }
        public string Value { get; }
        public SqlParameter(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
