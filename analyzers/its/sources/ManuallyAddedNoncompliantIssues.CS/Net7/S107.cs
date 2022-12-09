using System.Runtime.InteropServices;

namespace Net7
{
    public static unsafe partial class S107
    {
        public static void TooManyArguments(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8) // Noncompliant (S107): Method has 8 parameters, which is greater than the 7 authorized.
        {

        }

        [LibraryImport("foo.dll")]
        private static partial void ExternWithoutMarshaling(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8); // Compliant, external definition

        [LibraryImport("nativelib", EntryPoint = "ExternWithMarshaling", StringMarshalling = StringMarshalling.Utf16)]
        private static partial string ExternWithMarshaling(string str, int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8); // Compliant, external definition
    }
}
