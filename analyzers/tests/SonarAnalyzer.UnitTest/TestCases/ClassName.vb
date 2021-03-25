Namespace Tests.Diagnostics
    Public Class myClassName ' Noncompliant {{Rename this class to match the regular expression: '^([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?$'.}}
'                ^^^^^^^^^^^
    End Class

    Public Class [myClassName1] ' Noncompliant
'                ^^^^^^^^^^^^^^
    End Class

    Public Class [my_Class_Name1] ' Noncompliant
'                ^^^^^^^^^^^^^^^^
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
