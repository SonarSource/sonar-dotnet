Namespace Tests.TestCases

    Public Class Program

        Private empty As String = ""

        Public Const NameConst As String = "foobar" ' Noncompliant {{Define a constant instead of using this literal 'foobar' 7 times.}}
        '                                  ^^^^^^^^
        Public ReadOnly NameReadonly As String = "foobar"
        '                                        ^^^^^^^^ Secondary

        Private name As String = "foobar"
        '                        ^^^^^^^^ Secondary

        Private values As String() = {"something", "something", "something"} ' Compliant - repetition below threshold

        Private ReadOnly Property FirstName = "foobar"
        '                                     ^^^^^^^^ Secondary

        Public Sub New()

            Dim x As String = "foobar"
            '                 ^^^^^^^^ Secondary

            Dim y As String = "FooBar" ' Compliant - casing Is different

        End Sub

        Public Sub DoSomething(Optional s As String = "foobar")
            '                                         ^^^^^^^^ Secondary
            Dim x As String = If(s, "foobar")
            '                       ^^^^^^^^ Secondary
        End Sub

        Public Sub Validate(fOobAR As Object)
            If fOobAR Is Nothing Then Throw New System.ArgumentNullException("foobar") ' Compliant - matches one of the parameter name

            DoSomething("foobar") ' Compliant - matches one of the parameter name

        End Sub

    End Class

    Public Class OuterClass

        Private ReadOnly Property Name As String = "foobar" ' Noncompliant

        Private Class InnerClass

            Private name1 As String = "foobar" ' Secondary - inner class count with base
            Private name2 As String = "foobar" ' Secondary
            Private name3 As String = "foobar" ' Secondary
            Private name4 As String = "foobar" ' Secondary

        End Class

        Private Structure InnerStruct

            Private name1 As String
            Private name2 As String
            Private name3 As String
            Private name4 As String

            Public Sub New(s As String)

                name1 = "foobar" ' Secondary - inner struct count with base
                name2 = "foobar" ' Secondary
                name3 = "foobar" ' Secondary
                name4 = "foobar" ' Secondary

            End Sub

        End Structure

    End Class

    Public Structure OuterStruct

        Private Name As String
        Public Sub New(s As String)

            Name = "foobar" ' Noncompliant

        End Sub

        Private Structure InnerStruct

            Private name1 As String
            Private name2 As String
            Private name3 As String
            Private name4 As String

            Public Sub New(s As String)
                name1 = "foobar" ' Secondary - inner struct count with base
                name2 = "foobar" ' Secondary
                name3 = "foobar" ' Secondary
                name4 = "foobar" ' Secondary
            End Sub

        End Structure

    End Structure

    Public Class SpecialChars
        Private someString1 As String = "Say ""Hello""" ' Noncompliant {{Define a constant instead of using this literal 'Say "Hello"' 4 times.}}
        Private someString2 As String = "Say ""Hello""" ' Secondary
        Private someString3 As String = "Say ""Hello""" ' Secondary
        Private someString4 As String = "Say ""Hello""" ' Secondary
    End Class

End Namespace
