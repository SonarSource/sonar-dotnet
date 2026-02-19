Imports System
Imports System.Net
Imports System.Security
Imports System.Security.Cryptography

Namespace Tests.Diagnostics

    Class Program

        Public Const DBConnectionString As String = "Server=localhost; Database=Test; User=SA; Password=Secret123"    ' Noncompliant
        Public Const EditPasswordPageUrlToken As String = "{Account.EditPassword.PageUrl}" ' Compliant

        Private Const SecretConst As String = "constantValue"
        Private SecretField As String = "literalValue"
        Dim SecretFieldConst As String = SecretConst
        Private SecretFieldUninitialized As String
        Private SecretFieldNull As String = Nothing
        Private SecretFieldMethod As String = SomeMethod()

        Private Shared Function SomeMethod() As String
            Return ""
        End Function

        Public Sub Test(User As String)
            Dim password As String = "foo" 'Noncompliant {{"password" detected here, make sure this is not a hard-coded credential.}}
            '   ^^^^^^^^^^^^^^^^^^^^^^^^^^
            Dim foo As String, passwd As String = "a" 'Noncompliant {{"passwd" detected here, make sure this is not a hard-coded credential.}}
            '                  ^^^^^^^^^^^^^^^^^^^^^^
            Dim pwdPassword As String = "a"     'Noncompliant {{"pwd and password" detected here, make sure this is not a hard-coded credential.}}
            Dim foo2 As String = "Password=123" 'Noncompliant
            Dim bar As String
            bar = "Password=p" 'Noncompliant ^13#18

            Dim obj As Object
            obj = "Password=p" ' Compliant, only assignment To String Is inspected

            foo = "password"
            foo = "password="
            foo = "passwordpassword"
            foo = "foo=1;password=1"    'Noncompliant
            foo = "foo=1password=1"     'Noncompliant
            foo = "userpassword=1"      'Noncompliant
            foo = "passwordfield=1"     'Compliant

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

        Public Sub Concatenations(Arg As String)
            Dim SecretVariable As String = "literalValue"
            Dim SecretVariableConst As String = SecretConst
            Dim SecretVariableNull As String = Nothing
            Dim SecretVariableMethod As String = SomeMethod()
            Dim a As String

            a = "Server = localhost; Database = Test; User = SA; Password = " & "hardcoded"           ' Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " + "hardcoded"           ' Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " & SecretConst           ' Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " + SecretConst           ' Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " & SecretField           ' Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " + SecretField           ' Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " & SecretFieldConst      ' Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " & SecretVariable        ' Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " + SecretVariable        ' Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " & SecretVariableConst   ' Noncompliant

            a = "Server = localhost; Database = Test; User = SA; Password = " & SecretFieldUninitialized  ' Compliant, not initialized to constant
            a = "Server = localhost; Database = Test; User = SA; Password = " & SecretFieldNull           ' Compliant, not initialized to constant
            a = "Server = localhost; Database = Test; User = SA; Password = " & SecretVariableNull        ' Compliant, not initialized to constant
            a = "Server = localhost; Database = Test; User = SA; Password = " & SecretVariableMethod      ' Compliant, not initialized to constant
            a = "Server = localhost; Database = Test; User = SA; Password = " & Arg                       ' Compliant, not initialized to constant

            Const PasswordPrefixConst As String = "Password = "         ' Compliant by it's name
            Dim PasswordPrefixVariable As String = "Password = "        ' Compliant by it's name
            a = "Server = localhost;" & " Database = Test; User = SA; Password = " & SecretConst                ' Noncompliant
            a = "Server = localhost;" + " Database = Test; User = SA; Password = " + SecretConst                ' Noncompliant
            a = "Server = localhost;" & " Database = Test; User = SA; Pa" & "ssword = " & SecretConst           ' FN, we don't track all concatenations to avoid duplications
            a = "Server = localhost;" & " Database = Test; User = SA; " & PasswordPrefixConst & SecretConst     ' Noncompliant
            a = "Server = localhost;" & " Database = Test; User = SA; " & PasswordPrefixVariable & SecretConst  ' Noncompliant
            a = "Server = localhost;" & " Database = Test; User = SA; Password = " & SecretConst & " suffix"    ' Noncompliant
            '   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            a = SomeMethod() & " Database = Test; User = SA; Password = " & SecretConst & " suffix"             ' Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " & SecretConst & Arg & " suffix"   ' Noncompliant
            a = "Server = localhost; Database = Test; User = SA; Password = " & Arg & SecretConst & " suffix"   ' Compliant
            a = SecretConst & "Server = localhost; Database = Test; User = SA; Password = " & Arg               ' Compliant
            a = "Server = localhost; Database = Test; User = SA; " & SomeMethod() & SecretConst                 ' Compliant

            ' Reassigned
            Arg &= "Literal"
            a = "Server = localhost; Database = Test; User = SA; Password = " & Arg                     ' Compliant, &= is not a constant propagating operation
            SecretVariableMethod = "literal"
            a = "Server = localhost; Database = Test; User = SA; Password = " & SecretVariableMethod    ' Noncompliant
            Arg = "literal"
            a = "Server = localhost; Database = Test; User = SA; Password = " & Arg                     ' Noncompliant
            SecretVariable = SomeMethod()
            a = "Server = localhost; Database = Test; User = SA; Password = " & SecretVariable          ' Compliant
        End Sub

        Public Sub Interpolations(Arg As String)
            Dim SecretVariable As String = "literalValue"
            Dim a As String
            a = $"Server = localhost; Database = Test; User = SA; Password = {SecretConst}"         ' Noncompliant
            a = $"Server = localhost; Database = Test; User = SA; Password = {SecretField}"         ' Noncompliant
            a = $"Server = localhost; Database = Test; User = SA; Password = {SecretVariable}"      ' Noncompliant
            a = $"Server = localhost; Database = Test; User = SA; Password = {Arg}"                 ' Compliant
            a = $"Server = localhost; Database = Test; User = SA; Password = {Arg}{SecretConst}"    ' Compliant
        End Sub

        Public Sub StringFormat(Arg As String, FormatProvider As IFormatProvider, Arr() As String)
            Dim SecretVariable As String = "literalValue"
            Dim a As String

            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {0}", SecretConst)            ' Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {1}", Nothing, SecretConst)   ' Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {1}", Nothing, SecretField)   ' Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {2}", 0, 0, SecretVariable)   ' Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {0}", Arg)                    ' Compliant
            a = String.Format(FormatProvider, "Database = Test; User = SA; Password = {0}", SecretConst)                ' Compliant, we can't simulate formatProvider behavior
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {0}", Arr)                    ' Compliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {invalid}", SecretConst)      ' Compliant, the format is invalid and we should not raise
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = invalid {0", SecretConst)     ' Compliant, the format is invalid and we should not raise
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {0:#,0.00}", Arg)             ' Compliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {0}{1}{2}", Arg)              ' Compliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = hardcoded")                   ' Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = {0}; Password = hardcoded", Arg)             ' Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {1}{0}", Arg, SecretConst)    ' Noncompliant
            a = String.Format("Server = localhost; Database = Test; User = SA; Password = {0}{1}", Arg, SecretConst)    ' Compliant
            a = String.Format("{0} Argument 1 is not used", "Hello", "User = SA; Password = hardcoded")                 ' Compliant

            a = String.Format(arg0:=SecretConst, format:="Server = localhost; Database = Test; User = SA; Password = {0}")  ' FN, not supported
        End Sub

        Private Sub ByRefVariable()
            Dim Secret As String = "hardcoded"
            FillByRef(Secret)
            Dim A As String = "Server = localhost; Database = Test; User = SA; Password = " & Secret   ' Noncompliant FP
        End Sub

        Private Sub FillByRef(ByRef Arg As String)
            Arg = SomeMethod()
        End Sub

        Public Sub StandardAPI(secureString As SecureString, nonHardcodedPassword As String, byteArray As Byte(), cspParams As CspParameters)
            Const SecretLocalConst As String = "hardcodedSecret"
            Dim SecretVariable As String = "literalValue"
            Dim SecretVariableConst As String = SecretConst
            Dim SecretVariableNull As String = Nothing
            Dim SecretVariableMethod As String = SomeMethod()
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

            networkCredential = New NetworkCredential("username", SecretConst)           ' Noncompliant {{Please review this hard-coded password.}}
            networkCredential = New NetworkCredential("username", SecretLocalConst)      ' Noncompliant {{Please review this hard-coded password.}}
            networkCredential = New NetworkCredential("username", SecretField)           ' Noncompliant
            networkCredential = New NetworkCredential("username", SecretFieldConst)      ' Noncompliant
            networkCredential = New NetworkCredential("username", SecretVariable)        ' Noncompliant
            networkCredential = New NetworkCredential("username", SecretVariableConst)   ' Noncompliant
            networkCredential = New NetworkCredential("username", "hardcoded")           ' Noncompliant {{Please review this hard-coded password.}}
            networkCredential = New NetworkCredential("username", "hardcoded", "domain") ' Noncompliant {{Please review this hard-coded password.}}
            networkCredential.Password = "hardcoded"                                     ' Noncompliant {{Please review this hard-coded password.}}
            networkCredential.Password = SecretField                                     ' Noncompliant
            networkCredential.Password = SecretVariable                                  ' Noncompliant
            passwordDeriveBytes = New PasswordDeriveBytes("hardcoded", byteArray)                               'Noncompliant {{Please review this hard-coded password.}}
            passwordDeriveBytes = New PasswordDeriveBytes("hardcoded", byteArray, cspParams)                    'Noncompliant {{Please review this hard-coded password.}}
            passwordDeriveBytes = New PasswordDeriveBytes("hardcoded", byteArray, "strHashName", 1)             'Noncompliant {{Please review this hard-coded password.}}
            passwordDeriveBytes = New PasswordDeriveBytes("hardcoded", byteArray, "strHashName", 1, cspParams)  'Noncompliant {{Please review this hard-coded password.}}

            ' Compliant
            networkCredential = New NetworkCredential("username", SecretFieldUninitialized)
            networkCredential = New NetworkCredential("username", SecretFieldNull)
            networkCredential = New NetworkCredential("username", SecretFieldMethod)
            networkCredential = New NetworkCredential("username", SecretVariableNull)
            networkCredential = New NetworkCredential("username", SecretVariableMethod)
            networkCredential.Password = SecretFieldMethod
            networkCredential.Password = SecretVariableMethod
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
            Dim c9 As String = $"scheme://user:{SecretConst}@domain.com"    ' Noncompliant

            Dim e1 As String = "scheme://admin:admin@domain.com"    ' Compliant exception, user and password are the same
            Dim e2 As String = "scheme://abc:abc@domain.com"        ' Compliant exception, user and password are the same
            Dim e3 As String = "scheme://a%20;c:a%20;c@domain.com"  ' Compliant exception, user and password are the same
            Dim e4 As String = "scheme://user:;@domain.com"         ' Compliant exception for implementation purposes

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
            Dim query1 As String = "password=':crazy;secret';user=xxx" ' Noncompliant
        End Sub

        Class Configuration
            Public Shared Property Password As String
        End Class
    End Class

End Namespace
