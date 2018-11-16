using System.Web.Mvc;

public class Foo : Controller
{
    public Foo() { } // Compliant, ctors should not be highlighted

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

    private class Bar : Controller
    {
        public void InnerFoo() { }
    }
}
