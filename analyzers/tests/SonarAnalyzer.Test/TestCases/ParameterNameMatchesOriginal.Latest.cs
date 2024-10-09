namespace CSharp9
{
    public partial record Record
    {
        partial void M1(int x, int y);

        private partial void M2(int x, int y);
        internal partial void M3(int x, int y);
        protected partial void M4(int x, int y);
        protected internal partial void M5(int x, int y);
        public partial void M6(int x, int y);
        public partial bool M7(string x, out string y);
    }

    public partial record Record
    {
        private partial void M2(int someParam, int y) { } //Noncompliant {{Rename parameter 'someParam' to 'x' to match the partial class declaration.}}
        internal partial void M3(int someParam, int y) { } //Noncompliant {{Rename parameter 'someParam' to 'x' to match the partial class declaration.}}
        protected partial void M4(int someParam, int y) { } //Noncompliant {{Rename parameter 'someParam' to 'x' to match the partial class declaration.}}
        protected internal partial void M5(int someParam, int y) { } //Noncompliant {{Rename parameter 'someParam' to 'x' to match the partial class declaration.}}
        public partial void M6(int someParam, int y) { } //Noncompliant {{Rename parameter 'someParam' to 'x' to match the partial class declaration.}}
        public partial bool M7(string someParam, out string y) { y = string.Empty; return true; } //Noncompliant {{Rename parameter 'someParam' to 'x' to match the partial class declaration.}}
    }

    public abstract record BaseRecord<T>
    {
        public abstract void SomeMethod(T someParameter);
        public abstract void SomeMethod(T someParameter, int anotherParameter);
    }

    public record RecordOne : BaseRecord<int>
    {
        public override void SomeMethod(int renamedParam) { }
        public override void SomeMethod(int renamedParam, int wrongName) { } //Noncompliant {{Rename parameter 'wrongName' to 'anotherParameter' to match the base class declaration.}}
    }
}

namespace CSharp11
{
    public interface IAnotherInterface
    {
        static abstract void DoSomething(string value);
        static abstract void DoSomethingElse(string value);
    }

    public class AnotherClass : IAnotherInterface
    {
        public static void DoSomething(string renamedParam) // Noncompliant
        {
        }

        public static void DoSomethingElse(string value)
        {
        }
    }
}

namespace CSharp13
{
    public partial class PartialIndexers
    {
        public partial int this[int x] { get; }
    }
}
