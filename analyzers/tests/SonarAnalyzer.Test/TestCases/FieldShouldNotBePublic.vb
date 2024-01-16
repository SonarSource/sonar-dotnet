Namespace Tests.TestCases
    Public Class SomeClass
        Public Shared Pi As Double = 3.14
        Public Const Pi2 As Double = 3.14
        Public ReadOnly Pi3 As Double = 3.14 ' Noncompliant {{Make 'Pi3' private.}}
'                       ^^^
        Public Pi4 As Double = 3.14 ' Noncompliant
        Private Pi5 As Double = 3.14
        Protected Pi6 As Double = 3.14
        Friend Pi7 As Double = 3.14

        Public Class MyPublicSubClass

            Public Pi4 As Double = 3.14 ' Noncompliant
            Private Pi5 As Double = 3.14
            Protected Pi6 As Double = 3.14
            Friend Pi7 As Double = 3.14
        End Class

        Private Class MyPrivateSubClass

            Public Pi4 As Double = 3.14
            Private Pi5 As Double = 3.14
            Protected Pi6 As Double = 3.14
            Friend Pi7 As Double = 3.14
        End Class

        Protected Class MyProtectedSubClass

            Public Pi4 As Double = 3.14 ' Noncompliant
            Private Pi5 As Double = 3.14
            Protected Pi6 As Double = 3.14
            Friend Pi7 As Double = 3.14
        End Class

        Friend Class MyInternalSubClass

            Public Pi4 As Double = 3.14
            Private Pi5 As Double = 3.14
            Protected Pi6 As Double = 3.14
            Friend Pi7 As Double = 3.14
        End Class
    End Class

    Public Structure MyStruct
        Public Shared Pi As Double = 3.14
        Public Const Pi2 As Double = 3.14

        Public Pi3 As Double
        Private Pi4 As Double
        Friend Pi5 As Double

        Public Structure MyPublicSubStruct
            Public Pi4 As Double
            Private Pi5 As Double
            Friend Pi7 As Double
        End Structure

        Private Structure MyPrivateSubStruct
            Public Pi4 As Double
            Private Pi5 As Double
            Friend Pi7 As Double
        End Structure

        Friend Structure MyInternalSubStruct
            Public Pi4 As Double
            Private Pi5 As Double
            Friend Pi7 As Double
        End Structure
    End Structure
End Namespace
