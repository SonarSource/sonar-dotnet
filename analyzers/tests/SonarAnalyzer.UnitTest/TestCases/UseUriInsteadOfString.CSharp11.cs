using System;

public class Foo
{
    public Foo(string uri) { }
    public Foo(Uri uri) { }

    public void Method(string uri) => Method(new Uri(uri));
    public void Method(Uri uri) { }

    public void Test()
    {
        new Foo("""www.sonarsource.com""");              // Noncompliant {{Call the overload that takes a 'System.Uri' as an argument instead.}}
        new Foo("""
            www.sonarsource.com
            """);                                        // Noncompliant@-2
        new Foo($$"""
            www.sonarsource{{
                1 + 1
            }}.com
            """);                                        // Noncompliant@-4
        Method("""www.sonarsource.com""");               // Noncompliant
        Method($$"""www.sonarsource{{1 + 1}}.com""");    // Noncompliant
        Method($"www.sonarsource{                        // Noncompliant
                1 + 1
            }.com");
    }
}
