using System.Web.Mvc;

public class Foo : Controller
{
    public void PublicFoo() { } // Noncompliant {{Make sure that exposing this HTTP endpoint is safe here.}}
//              ^^^^^^^^^

    protected void ProtectedFoo() { }

    internal void InternalFoo() { }

    private void PrivateFoo() { }

    private class Bar : Controller
    {
        public void InnerFoo() { }
    }
}
