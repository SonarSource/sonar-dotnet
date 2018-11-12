using Microsoft.AspNetCore.Mvc;

public class SomeController : ControllerBase
{
    public void PublicFoo() { } // Noncompliant {{Make sure that exposing this HTTP endpoint is safe here.}}
//              ^^^^^^^^^

    protected void ProtectedFoo() { }

    internal void InternalFoo() { }

    private void PrivateFoo() { }

    private class Bar : ControllerBase
    {
        public void InnerFoo() { }
    }
}

[Controller]
public class MyController
{
    public void PublicFoo() { } // Noncompliant
}

[NonController]
public class MyNoncontroller : ControllerBase
{
    public void PublicFoo() { } // Compliant, not a controller
}
