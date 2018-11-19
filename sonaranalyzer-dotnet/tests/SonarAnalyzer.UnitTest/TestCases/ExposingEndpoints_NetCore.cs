using Microsoft.AspNetCore.Mvc;

public class SomeController : ControllerBase
{
    public SomeController() { } // Compliant, ctors should not be highlighted

    public int MyProperty // Compliant, properties should not be highlighted
    {
        get { return 0; }
        set { }
    }

    public void PublicFoo() { } // Noncompliant {{Make sure that exposing this HTTP endpoint is safe here.}}
//              ^^^^^^^^^

    protected void ProtectedFoo() { }

    internal void InternalFoo() { }

    private void PrivateFoo() { }

    [NonAction]
    public void PublicNonAction() { } // Compliant, methods decorated with NonAction are not entrypoints

    private class Bar : ControllerBase
    {
        public void InnerFoo() { }
    }
}

[Controller]
public class MyController
{
    public MyController() { } // Compliant, ctor should not be highlighted

    public void PublicFoo() { } // Noncompliant
}

[NonController]
public class MyNoncontroller : ControllerBase
{
    public void PublicFoo() { } // Compliant, not a controller
}
