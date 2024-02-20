using System;

namespace CSharpLatest.CSharp9
{
    public class S4005
    {
        Foo foo = new ("www.sonarsource.com");
        record Foo
        {
            public Foo(string uri) { }
            public Foo(Uri uri) { }
        }
    }
}
