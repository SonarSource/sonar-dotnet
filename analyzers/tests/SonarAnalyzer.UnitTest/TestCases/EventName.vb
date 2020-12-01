Namespace Tests.Diagnostics
    Module Module1
        Event fooEvent() ' Noncompliant
        Event FooEvent2() ' Compliant
        Event FooEvent2XX() ' Compliant
    End Module
End Namespace