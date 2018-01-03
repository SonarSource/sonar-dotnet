using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

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
            if (object.Equals(t[0], default(T))) // Fixed
            {
            }
        }
        public void My(ImmutableArray<B> analyzers)
        {
            if (t.Any(x => x == null)) //compliant, B is a class
            {
            }
        }
        public void Mx(List<C> t)
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
            if (object.Equals(t, default(T))) // Fixed
            {
            }
            if (!object.Equals(t, default(T))) // Fixed
            {
            }
        }
        public void M4<T>(T t) where T : A
        {
            if (t == null)
            {
            }
        }
        public void M4<T>(T t) where T : C
        {
            if (t == null)
            {
            }
        }
    }
}
