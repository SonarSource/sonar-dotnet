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
        Private someString1 As String = "Say ""Hello""" ' Noncompliant {{Define a constant instead of using this literal 'Say ""Hello""' 4 times.}}
        Private someString2 As String = "Say ""Hello""" ' Secondary
        Private someString3 As String = "Say ""Hello""" ' Secondary
        Private someString4 As String = "Say ""Hello""" ' Secondary
    End Class

End Namespace

' https://github.com/SonarSource/sonar-dotnet/issues/9569
Namespace SqlNamedParameters
    Public Class Program
        Public Sub ExecuteSqlCommands()
            Dim userCommand = New SqlCommand("SELECT * FROM Users WHERE Name = @Name")
            userCommand.AddParameter(New SqlParameter("@Name", "John Doe"))                    ' Noncompliant - FP: Name refers to parameters in different SQL tables.
            Dim users = userCommand.ExecuteQuery()                                             ' Renaming one does not necessitate renaming of parameters with the same name from other tables.

            Dim companyCommand = New SqlCommand("SELECT * FROM Companies WHERE Name = @Name")
            companyCommand.AddParameter(New SqlParameter("@Name", "Contosco"))                 ' Secondary - FP
            Dim companies = companyCommand.ExecuteQuery()

            Dim productCommand = New SqlCommand("SELECT * FROM Products WHERE Name = @Name")
            productCommand.AddParameter(New SqlParameter("@Name", "CleanBot 9000"))            ' Secondary - FP
            Dim products = productCommand.ExecuteQuery()

            Dim countryCommand = New SqlCommand("SELECT * FROM Countries WHERE Name = @Name")
            countryCommand.AddParameter(New SqlParameter("@Name", "Norway"))                   ' Secondary - FP
            Dim countries = countryCommand.ExecuteQuery()
        End Sub
    End Class

    Public Class SqlCommand
        Public ReadOnly Property CommandText As String
        Public Sub New(commandText As String)
            Me.CommandText = commandText
        End Sub

        Public Sub AddParameter(parameter As SqlParameter)
        End Sub

        Public Function ExecuteQuery() As Object
            Return Nothing
        End Function
    End Class

    Public Class SqlParameter
        Public ReadOnly Property Name As String
        Public ReadOnly Property Value As String
        Public Sub New(name As String, value As String)
            Me.Name = name
            Me.Value = value
        End Sub
    End Class
End Namespace
