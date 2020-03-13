Imports System
Imports System.Net
Imports System.Security
Imports System.Security.Cryptography

Namespace Tests.Diagnostics

    Class Program

        Public Const DBConnectionString As String = "Server=localhost; Database=Test; User=SA; Password=Secret123"    ' Noncompliant
        Public Const EditPasswordPageUrlToken As String = "{Account.EditPassword.PageUrl}" ' Compliant

        Private Const Secret As String = "constantValue"

        Public Sub Test(User As String)
            Dim password As String = "foo" 'Noncompliant {{"password" detected here, make sure this is not a hard-coded credential.}}
            '   ^^^^^^^^^^^^^^^^^^^^^^^^^^
            Dim foo As String, passwd As String = "a" 'Noncompliant {{"passwd" detected here, make sure this is not a hard-coded credential.}}
            '                  ^^^^^^^^^^^^^^^^^^^^^^
            Dim pwdPassword As String = "a"     'Noncompliant {{"pwd, password" detected here, make sure this is not a hard-coded credential.}}
            Dim foo2 As String = "Password=123" 'Noncompliant
            Dim bar As String
            bar = "Password=p" 'Noncompliant ^13#18

            foo = "password"
            foo = "password="
            foo = "passwordpassword"
            foo = "foo=1;password=1" 'Noncompliant
            foo = "foo=1password=1"

            Dim myPassword1 As String = Nothing
            Dim myPassword2 As String = ""
            Dim myPassword3 As String = "   "
            Dim myPassword4 As String = "foo" 'Noncompliant
            Dim query2 As String = "password=hardcoded;user='" + User + "'" ' Noncompliant
        End Sub

        Public Sub DefaultKeywords()
            Dim password As String = "a"       ' Noncompliant
            Dim x1 As String = "password=a"    ' Noncompliant

            Dim passwd As String = "a"         ' Noncompliant
            Dim x2 As String = "passwd=a"      ' Noncompliant

            Dim pwd As String = "a"            ' Noncompliant
            Dim x3 As String = "pwd=a"         ' Noncompliant

            Dim passphrase As String = "a"     ' Noncompliant
            Dim x4 As String = "passphrase=a"  ' Noncompliant
        End Sub

        Public Sub Constants()
            Const ConnectionString As String = "Server=localhost; Database=Test; User=SA; Password=Secret123"    ' Noncompliant
            Const ConnectionStringWithSpaces As String = "Server=localhost; Database=Test; User=SA; Password   =   Secret123"    ' Noncompliant
            Const Password As String = "Secret123"  ' Noncompliant

            Const LoginName As String = "Admin"
            Const Localhost As String = "localhost"
        End Sub

        Public Sub StandardAPI(secureString As SecureString, nonHardcodedPassword As String, byteArray As Byte(), cspParams As CspParameters)
            Dim networkCredential = New NetworkCredential()
            networkCredential.Password = nonHardcodedPassword
            networkCredential.Domain = "hardcodedDomain"
            networkCredential = New NetworkCredential("username", secureString)
            networkCredential = New NetworkCredential("username", nonHardcodedPassword)
            networkCredential = New NetworkCredential("username", secureString, "domain")
            networkCredential = New NetworkCredential("username", nonHardcodedPassword, "domain")

            Dim passwordDeriveBytes = New PasswordDeriveBytes(nonHardcodedPassword, byteArray)
            passwordDeriveBytes = New PasswordDeriveBytes(New Byte() {1}, byteArray)
            passwordDeriveBytes = New PasswordDeriveBytes(nonHardcodedPassword, byteArray, cspParams)
            passwordDeriveBytes = New PasswordDeriveBytes(New Byte() {1}, byteArray, cspParams)
            passwordDeriveBytes = New PasswordDeriveBytes(nonHardcodedPassword, byteArray, "strHashName", 1)
            passwordDeriveBytes = New PasswordDeriveBytes(New Byte() {1}, byteArray, "strHashName", 1)
            passwordDeriveBytes = New PasswordDeriveBytes(nonHardcodedPassword, byteArray, "strHashName", 1, cspParams)
            passwordDeriveBytes = New PasswordDeriveBytes(New Byte() {1}, byteArray, "strHashName", 1, cspParams)

            networkCredential = New NetworkCredential("username", Secret) 'Noncompliant {{Please review this hard-coded password.}}
            networkCredential = New NetworkCredential("username", "hardcoded") 'Noncompliant {{Please review this hard-coded password.}}
            networkCredential = New NetworkCredential("username", "hardcoded", "domain") 'Noncompliant {{Please review this hard-coded password.}}
            networkCredential.Password = "hardcoded" 'Noncompliant {{Please review this hard-coded password.}}
            passwordDeriveBytes = New PasswordDeriveBytes("hardcoded", byteArray) 'Noncompliant {{Please review this hard-coded password.}}
            passwordDeriveBytes = New PasswordDeriveBytes("hardcoded", byteArray, cspParams) 'Noncompliant {{Please review this hard-coded password.}}
            passwordDeriveBytes = New PasswordDeriveBytes("hardcoded", byteArray, "strHashName", 1) 'Noncompliant {{Please review this hard-coded password.}}
            passwordDeriveBytes = New PasswordDeriveBytes("hardcoded", byteArray, "strHashName", 1, cspParams) 'Noncompliant {{Please review this hard-coded password.}}
        End Sub

        Public Sub CompliantParameterUse(pwd As String)
            Dim query1 As String = "password=?"
            Dim query2 As String = "password=:password"
            Dim query3 As String = "password=:param"
            Dim query4 As String = "password='" + pwd + "'"
            Dim query5 As String = "password='" & pwd & "'"
            Dim query6 As String = "password={0}"
            Dim query7 As String = "password=;user=;"
            Dim query8 As String = "password=:password;user=:user;"
            Dim query9 As String = "password=?;user=?;"
            Dim query10 As String = "Server=myServerName\myInstanceName;Database=myDataBase;Password=:myPassword;User Id=:username;"
            Using Conn As New SqlConnection("Server = localhost; Database = Test; User = SA; Password = ?")
            End Using
            Using Conn As New SqlConnection("Server = localhost; Database = Test; User = SA; Password = :password")
            End Using
            Using Conn As New SqlConnection("Server = localhost; Database = Test; User = SA; Password = {0}")
            End Using
            Using Conn As New SqlConnection("Server = localhost; Database = Test; User = SA; Password = ")
            End Using
        End Sub

        Public Sub WordInConstantNameAndValue()
            ' It's compliant when the word is used in name AND the value.
            Const PASSWORD As String = "Password"
            Const Password_Input As String = "[id='password']"
            Const PASSWORD_PROPERTY As String = "custom.password"
            Const TRUSTSTORE_PASSWORD As String = "trustStorePassword"
            Const CONNECTION_PASSWORD As String = "connection.password"
            Const RESET_PASSWORD As String = "/users/resetUserPassword"
            Const RESET_PASSWORD_CS As String = "/uzivatel/resetovat-heslo" ' Noncompliant, "heslo" means "password", but we don't translate SEO friendly URL for all languages
        End Sub

        Public Sub WordInVariableNameAndValue()
            ' It's compliant when the word is used in name AND the value.
            Dim PasswordKey As String = "Password"
            Dim PasswordProperty As String = "config.password.value"
            Dim PasswordName As String = "UserPasswordValue"
            Dim Password As String = "Password"
            Dim pwd As String = "pwd"

            Dim myPassword As String = "pwd"    ' Noncompliant, different value from word list is used
        End Sub

        Public Sub UriWithUserInfo(Pwd As String, Domain As String)
            Dim n1 As String = "scheme://user:azerty123@domain.com" ' Noncompliant {{Review this hard-coded URI, which may contain a credential.}}
            Dim n2 As String = "scheme://user:With%20%3F%20Encoded@domain.com"              ' Noncompliant
            Dim n3 As String = "scheme://user:With!$&'()*+,;=OtherCharacters@domain.com"    ' Noncompliant
            Dim n4 As String = "scheme://user:azerty123@" + Domain  ' Noncompliant
            Dim n5 As String = "scheme://user:azerty123@" & Domain  ' Noncompliant

            Dim c1 As String = "scheme://user:" + Pwd + "@domain.com"
            Dim c2 As String = "scheme://user:" & Pwd & "@domain.com"
            Dim c3 As String = "scheme://user:@domain.com"
            Dim c4 As String = "scheme://user@domain.com:80"
            Dim c5 As String = "scheme://user@domain.com"
            Dim c6 As String = "scheme://domain.com/user:azerty123"
            Dim c7 As String = String.Format("scheme://user:{0}@domain.com", Pwd)
            Dim c8 As String = $"scheme://user:{Pwd}@domain.com"

            Dim e1 As String = "scheme://admin:admin@domain.com"    ' Compliant exception, user and password are the same
            Dim e2 As String = "scheme://abc:abc@domain.com"        ' Compliant exception, user and password are the same
            Dim e3 As String = "scheme://a%20;c:a%20;c@domain.com"  ' Compliant exception, user and password are the same

            Dim html1 As String = ' Noncompliant
"This is article http://login:secret@www.example.com
Email: info@example.com
Phone: +0000000"

            Dim html2 As String =
"This is article http://www.example.com
Email: info@example.com
Phone: +0000000"

            Dim html3 As String = "This is article http://www.example.com Email: info@example.com Phone: +0000000"
            Dim html4 As String = "This is article http://www.example.com<br>Email:info@example.com<br>Phone:+0000000"
            Dim html5 As String = "This is article http://user:secret@www.example.com<br>Email:info@example.com<br>Phone:+0000000" ' Noncompliant
        End Sub

        Public Sub LiteralAsArgument(pwd As String, server As String)
            Using Conn As New SqlConnection("Server = localhost; Database = Test; User = SA; Password = Secret123")  ' Noncompliant
            End Using
            Using Conn As SqlConnection = OpenConn("Server = localhost; Database = Test; User = SA; Password = Secret123") ' Noncompliant
            End Using
            Using Conn As New SqlConnection("Server = " + server + "; Database = Test; User = SA; Password = Secret123") ' Noncompliant
            End Using
            Using Conn As New SqlConnection("Server = " & server & "; Database = Test; User = SA; Password = Secret123") ' Noncompliant
            End Using

            Using OpenConn("password")
            End Using
            Using Conn As New SqlConnection("Server = localhost; Database = Test; User = SA; Password = " + pwd)
            End Using
            Using Conn As New SqlConnection("Server = localhost; Database = Test; User = SA; Password = " & pwd)
            End Using
        End Sub

        Private Function OpenConn(connectionString As String) As SqlConnection
            Dim Ret As New SqlConnection(connectionString)
            Ret.Open()
            Return Ret
        End Function

        Public ReadOnly Property ConnectionStringProperty As String
            Get
                Return "Server = localhost; Database = Test; User = SA; Password = Secret123" ' Noncompliant
            End Get
        End Property

        Public ReadOnly Property ConnectionStringProperty_OK As String
            Get
                Return "Nothing to see here"
            End Get
        End Property

        Public ReadOnly Property ConnectionStringProperty2 As String = "Server = localhost; Database = Test; User = SA; Password = Secret123" ' Noncompliant
        Public ReadOnly Property ConnectionStringProperty2_OK As String = "Nothing to see here"

        Public Function ConnectionStringFunction() As String
            Return "Server = localhost; Database = Test; User = SA; Password = Secret123" ' Noncompliant
        End Function

        Public Function ConnectionStringFunction_OK() As String
            Return "Nothing to see here"
        End Function

    End Class

    Public Class SqlConnection
        Implements IDisposable

        Public Sub New(ConnStr As String)
        End Sub

        Public Sub Open()
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub

    End Class

    Class FalseNegatives
        Private password As String

        Public Sub Foo(user As String)
            Me.password = "foo" ' False Negative
            Configuration.Password = "foo" ' False Negative
            Me.password = Configuration.Password = "foo" ' False Negative
            Dim query1 As String = "password=':crazy;secret';user=xxx" ' False Negative - passwords enclosed in '' are not covered
        End Sub

        Class Configuration
            Public Shared Property Password As String
        End Class
    End Class

End Namespace
