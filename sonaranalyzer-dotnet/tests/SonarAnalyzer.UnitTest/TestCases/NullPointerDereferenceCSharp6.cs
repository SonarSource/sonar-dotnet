using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class A { }

    class NullPointerDereferenceWithFieldsCSharp6 : A
    {
        private object _foo1;

        void ConditionalThisFieldAccess()
        {
            object o = null;
            this._foo1 = o;
            this?._foo1.ToString(); // Noncompliant
        }

        string TryCatch3()
        {
            object o = null;
            try
            {
                o = new object();
            }
            catch (Exception e) when (e.Message != null)
            {
                o = new object();
            }
            return o.ToString(); // Noncompliant, when e.Message is null o will be null
        }

        // https://github.com/SonarSource/sonar-csharp/issues/1324
        public void FlasePositive(object o)
        {
            try
            {
                var a = o?.ToString();
            }
            catch (InvalidOperationException) when (o != null)
            {
                var b = o.ToString(); // Compliant, o is checked for null in this branch
            }
            catch (ApplicationException) when (o == null)
            {
                var b = o.ToString(); // Noncompliant
            }
        }

        public void TryCatch4(object o)
        {
            try
            {
                var a = o?.ToString();
            }
            catch (Exception e) when (e.Message != null)
            {
                var b = o.ToString(); // Noncompliant, o could be null here
            }
        }
    }
}
