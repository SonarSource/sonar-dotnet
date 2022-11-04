namespace Tests.Diagnostics
{
    public interface IMyInterface
    {
        static abstract void Abstract(string[] messages);
        static abstract void Abstract(string[] messages, string delimiter = "\n"); // Noncompliant
//                                                       ^^^^^^^^^^^^^^^^^^^^^^^
        static virtual void Virtual(string[] messages) { }
        static virtual void Virtual(string[] messages, string delimiter = "\n") { } // Noncompliant
    }

    public partial class MethodOverloadOptionalParameter : IMyInterface
    {
        public static void Abstract(string[] messages) { }
        public static void Abstract(string[] messages, string delimiter = "\n") { } // Compliant; comes from interface
        public static void Virtual(string[] messages) { }
        public static void Virtual(string[] messages, string delimiter = "\n") { } // Compliant; comes from interface

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
