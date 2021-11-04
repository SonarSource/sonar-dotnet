namespace Tests.Diagnostics
{
    public interface IMyInterface
    {
        static abstract void Print2(string[] messages);
        static abstract void Print2(string[] messages, string delimiter = "\n");// Noncompliant;
//                                                     ^^^^^^^^^^^^^^^^^^^^^^^
    }

    public partial class MethodOverloadOptionalParameter : IMyInterface
    {
        public static void Print2(string[] messages) { }
        public static void Print2(string[] messages, string delimiter = "\n") { } // Compliant; comes from interface

        partial void Print(string[] messages);

        partial void Print(string[] messages) { }

        void Print(string[] messages, string delimiter = "\n") { } // Noncompliant;
        void Print(string[] messages,
            string delimiter = "\n", // Noncompliant
            string b = "a" // Noncompliant
            )
        { }
    }
}
