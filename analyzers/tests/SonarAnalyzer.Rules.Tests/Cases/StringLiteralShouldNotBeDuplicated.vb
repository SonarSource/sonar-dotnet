Namespace Tests.TestCases

    Public Class Program

        Private empty As String = ""

        Public const NameConst As String = "foobar" ' Noncompliant {{Define a constant instead of using this literal 'foobar' 7 times.}}
        '                                  ^^^^^^^^
        Public ReadOnly NameReadonly As String = "foobar"
        '                                        ^^^^^^^^ Secondary

        Private name As String = "foobar"
        '                        ^^^^^^^^ Secondary

        Private values As String() = { "something", "something", "something" } ' Compliant - repetition below threshold

        Private ReadOnly Property FirstName = "foobar"
        '                                     ^^^^^^^^ Secondary

        Public Sub New()

            Dim x As String = "foobar"
            '                 ^^^^^^^^ Secondary

            Dim y As String = "FooBar" ' Compliant - casing Is different

        End Sub

        public Sub DoSomething(Optional s As String = "foobar")
        '                                             ^^^^^^^^ Secondary
            Dim x As String = If(s, "foobar")
            '                       ^^^^^^^^ Secondary
        End Sub

        public Sub Validate(fOobAR As Object)
             If foobar Is Nothing Then Throw New System.ArgumentNullException("foobar") ' Compliant - matches one of the parameter name

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

            private name1 As String
            private name2 As String
            private name3 As String
            private name4 As String

            public Sub New(s As String)

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
        
            private name1 As String
            private name2 As String
            private name3 As String
            private name4 As String

            public Sub New(s As String)
                name1 = "foobar" ' Secondary - inner struct count with base
                name2 = "foobar" ' Secondary
                name3 = "foobar" ' Secondary
                name4 = "foobar" ' Secondary
            End Sub

        End Structure

    End Structure

End Namespace
