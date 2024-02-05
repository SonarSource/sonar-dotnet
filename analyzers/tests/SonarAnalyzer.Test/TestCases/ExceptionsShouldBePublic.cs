using System;
using System.Collections.Generic;

public class PublicException : Exception // Compliant
{
}

internal class InternalException : Exception
//             ^^^^^^^^^^^^^^^^^ Noncompliant {{Make this exception 'public'.}}
{
}

class InternalException2 : Exception
//    ^^^^^^^^^^^^^^^^^^ Noncompliant {{Make this exception 'public'.}}
{
}

public class PublicContainer // Compliant
{
    public class PublicException : Exception // Compliant
    {
    }

    private class PrivateClass // Compliant
    {
    }

    internal class InternalException : Exception
//                 ^^^^^^^^^^^^^^^^^ Noncompliant {{Make this exception 'public'.}}
    {
    }

    class InternalException2 : Exception
//        ^^^^^^^^^^^^^^^^^^ Noncompliant {{Make this exception 'public'.}}
    {
    }

    protected class ProtectedException : OutOfMemoryException // Compliant
    {
    }

    private class PrivateException2 : Exception
//                ^^^^^^^^^^^^^^^^^ Noncompliant {{Make this exception 'public'.}}
    {
    }
}

internal class InternalContainer // Compliant
{
    public class PublicException : Exception
//               ^^^^^^^^^^^^^^^ Noncompliant {{Make this exception 'public'.}}
    {
    }

    private class PrivateClass // Compliant
    {
    }

    protected class ProtectedClass // Compliant
    {
    }

    internal class InternalException : OutOfMemoryException // Compliant
    {
    }

    protected class ProtectedException : OutOfMemoryException // Compliant
    {
    }

    class InternalException2 : ApplicationException
//        ^^^^^^^^^^^^^^^^^^ Noncompliant {{Make this exception 'public'.}}
    {
    }

    private class PrivateException2 : Exception
//                ^^^^^^^^^^^^^^^^^ Noncompliant {{Make this exception 'public'.}}
    {
    }
}

internal class LastException : MiddleException { }
public class MiddleException : FirstException { }
public class FirstException : Exception { }

namespace ShouldNotThrow
{
    public class /* Missing identifier */ : Exception { } // Error [CS1001]
}
