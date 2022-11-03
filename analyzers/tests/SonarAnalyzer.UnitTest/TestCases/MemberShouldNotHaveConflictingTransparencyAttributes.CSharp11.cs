using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

[SecurityCritical]
// Secondary@-1
// Secondary@-2
public interface Foo
{
    [SecuritySafeCritical] // Noncompliant
    public static virtual void Bar()
    {
    }

    [SecuritySafeCritical] // Noncompliant
    public static abstract void Bar2();
}
