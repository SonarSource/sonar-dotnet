using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    abstract class RedundantDeclaration
    {
        public void M()
        {
            Test(null, new BoolDelegate(() => true)); // Compliant (natural type is Func<bool> and not BoolDelegate)
            Test(null, new Func<bool>(() => true));   // Noncompliant
        }

        public abstract void Test(object o, Delegate f);
        public delegate bool BoolDelegate();
    }

    // https://sonarsource.atlassian.net/browse/NET-3456
    class Repro_NET3456
    {
        [AttributeUsage(AttributeTargets.Parameter)]
        class BindingAttribute : Attribute { }

        void Test()
        {
            Action<int> withoutAttribute = (x) => { };           // Noncompliant {{'x' is not used. Use discard parameter instead.}}
            Action<int> withAttribute    = ([Binding] x) => { }; // Noncompliant - FP: x is annotated; the attribute may be consumed by a framework (e.g. [FromRoute], [JsonProperty])
        }
    }
}
