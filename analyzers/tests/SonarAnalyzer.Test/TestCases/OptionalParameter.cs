using System.Runtime.CompilerServices;

namespace Tests.Diagnostics
{
    public interface IInterface
    {
        void Method(int i = 42); //Noncompliant
//                        ^^^^
    }

    public class Base
    {
        public virtual void Method(int i = 42) //Noncompliant {{Use the overloading mechanism instead of the optional parameters.}}
        { }
    }

    public class OptionalParameter : Base
    {
        public override void Method(int i = 42) //Compliant
        {
            base.Method(i);
        }
        public OptionalParameter(int i = 0, // Noncompliant
            int j = 1) // Noncompliant
        {
        }
        public OptionalParameter()
        {
        }
        private OptionalParameter(int i = 0) // Compliant, private
        {
        }

        private void PrivateFoo(int i = 42) // Compliant, private
        { }

        internal void InternalFoo(int i = 42) // Compliant, internal
        { }

        protected void ProtectedFoo(int i = 42) // Noncompliant
        { }

        internal protected void InternalProtectedFoo(int i = 42) // Noncompliant
        {
        }

        public void PublicFoo(int i = 42) // Noncompliant
        { }
    }

    public class CallerMember
    {
        public void Caller_LineNumber([CallerLineNumber] int line = 0) { }
        public void Caller_FilePath([CallerFilePath] string sourceFilePath = "") { }
        public void Caller_MemberName([CallerMemberName] string memberName = "") { }
    }
}
