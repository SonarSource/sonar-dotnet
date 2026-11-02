namespace CSharp14
{
    public abstract partial class PartialConstructor
    {
        public partial PartialConstructor(PartialConstructor other);

        public abstract void DoSomething();
    }
}

namespace CSharp15
{
    public closed class ClosedBase
    {
        public ClosedBase()
        {
            DoSomething();  // Noncompliant {{Remove this call from a constructor to the overridable 'DoSomething' method.}}
            DoVirtual();    // Noncompliant
        }

        public abstract void DoSomething();

        public virtual void DoVirtual() { }
    }

    public class ClosedDerived : ClosedBase
    {
        public override void DoSomething() { }
    }

    public sealed class SealedClosedDerived : ClosedBase
    {
        public SealedClosedDerived()
        {
            DoSomething();  // Compliant
            DoVirtual();    // Compliant
        }

        public override void DoSomething() { }
    }
}
