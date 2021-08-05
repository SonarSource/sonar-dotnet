using System.Runtime.CompilerServices;

namespace Net5
{
    public class S3604
    {
        static int foo = 1;
        static int bar = 1;

        [ModuleInitializer]
        internal static void Initialize()
        {
            foo = 1;
        }

        static S3604()
        {
            bar = 1;
        }
    }

    public record S3604_Record(string s)
    {
        string foo = "foo";
        public S3604_Record() : this("x")
        {
            foo = "foo"; // handled by the C# compiler
        }
    }
}
