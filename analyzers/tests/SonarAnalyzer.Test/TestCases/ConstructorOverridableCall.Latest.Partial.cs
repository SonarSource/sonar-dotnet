using System;
namespace CSharp14
{
    public abstract partial class PartialConstructor
    {
        public partial PartialConstructor(PartialConstructor other)
        {
            DoSomething();          // Noncompliant {{Remove this call from a constructor to the overridable 'DoSomething' method.}}
//          ^^^^^^^^^^^
            this.DoSomething();     // Noncompliant
            other.DoSomething();    // Compliant

            var a = this;
            a.DoSomething();        // Not recognized

            var action = new Action(() => { DoSomething(); });
        }
    }
}
