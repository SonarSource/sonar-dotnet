using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class A
    {
    }

    public abstract class B
    {
    }

    public interface Interface
    {
    }

    public class GenericTypeParameterEmptinessChecking
    {
        public void M<T>(List<T> t)
        {
            if (t[0] == null) // Noncompliant {{Use a comparison to 'default(T)' instead or add a constraint to 'T' so that it can't be a value type.}}
//                      ^^^^
            {
            }
        }
        public void My(IEnumerable<B> analyzers)
        {
            if (analyzers.Any(x => x == null)) //compliant, B is a class
            {
            }
        }
        public void Mx(List<C> t) // Error [CS0246] - unknown type C
        {
            if (t.Any(x => x == null)) //compliant, we don't know anything about C
            {
            }
        }

        public void M2<T>(T t) where T : class
        {
            if (t == null)
            {
            }
        }
        public void M3<T>(T t) where T : Interface
        {
            if (null == t) // Noncompliant
            {
            }
            if (null != t) // Noncompliant
            {
            }
        }
        public void M4<T>(T t) where T : A
        {
            if (t == null)
            {
            }
        }
        public void M5<T>(T t) where T : C // Error [CS0246] - unknown type
        {
            if (t == null)
            {
            }
        }
    }
}
