Namespace Tests.Diagnostics
    Public Class myClassName ' Noncompliant
'                ^^^^^^^^^^^
    End Class

    Public Class [myClassName1] ' Noncompliant
'                ^^^^^^^^^^^^^^
    End Class

    Class MyClassName2

    End Class

    Public Structure myStructName ' Compliant
    End Structure

    Public Interface myInterfaceName ' Compliant
    End Interface

    Public Module myModuleName ' Compliant
    End Module

End Namespace