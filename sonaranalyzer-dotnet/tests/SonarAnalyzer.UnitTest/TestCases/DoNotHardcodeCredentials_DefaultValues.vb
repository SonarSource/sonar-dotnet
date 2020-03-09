Imports System
Imports System.Net
Imports System.Security
Imports System.Security.Cryptography

Namespace Tests.Diagnostics

    Class Program

        Public Const DBConnectionString As String = "Server=localhost; Database=Test; User=SA; Password=Secret123"    ' Noncompliant

        Private Const Secret As String = "constantValue"

        Public Sub Test()
            Dim password As String = "foo" 'Noncompliant {{"password" detected here, make sure this is not a hard-coded credential.}}
            '   ^^^^^^^^^^^^^^^^^^^^^^^^^^
            Dim foo As String, passwd As String = "a" 'Noncompliant {{"passwd" detected here, make sure this is not a hard-coded credential.}}
            '                  ^^^^^^^^^^^^^^^^^^^^^^
            Dim pwdPassword As String = "a"     'Noncompliant {{"pwd, password" detected here, make sure this is not a hard-coded credential.}}
            Dim foo2 As String = "Password=123" 'Noncompliant
            Dim bar As String
            bar = "Password=p" 'Noncompliant ^13#18
            foo = "password="
            foo = "passwordpassword"
            foo = "foo=1;password=1" 'Noncompliant
            foo = "foo=1password=1"

            Dim myPassword1 As String = Nothing
            Dim myPassword2 As String = ""
            Dim myPassword3 As String = "   "
            Dim myPassword4 As String = "foo" 'Noncompliant
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

            networkCredential = New NetworkCredential("username", Secret) 'Noncompliant
            networkCredential = New NetworkCredential("username", "hardcoded") 'Noncompliant
            networkCredential = New NetworkCredential("username", "hardcoded", "domain") 'Noncompliant
            networkCredential.Password = "hardcoded" 'Noncompliant
            passwordDeriveBytes = New PasswordDeriveBytes("hardcoded", byteArray) 'Noncompliant
            passwordDeriveBytes = New PasswordDeriveBytes("hardcoded", byteArray, cspParams) 'Noncompliant
            passwordDeriveBytes = New PasswordDeriveBytes("hardcoded", byteArray, "strHashName", 1) 'Noncompliant
            passwordDeriveBytes = New PasswordDeriveBytes("hardcoded", byteArray, "strHashName", 1, cspParams) 'Noncompliant
        End Sub

        Public Sub CompliantParameterUse(pwd As String)
            Dim query1 As String = "password=?"
            Dim query2 As String = "password=:password"
            Dim query3 As String = "password=:param"
            Dim query4 As String = "password='" + pwd + "'"
            Dim query5 As String = "password={0}"
            Dim query6 As String = "password=;user=;"
            Dim query7 As String = "password=:password;user=:user;"
            Dim query8 As String = "password=?;user=?;"
            Dim query9 As String = "Server=myServerName\myInstanceName;Database=myDataBase;Password=:myPassword;User Id=:username;"
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

            Dim fn1 As String = "scheme://user:azerty123@" & Domain  ' Compliant FN, concatenated strings are not supported

            Dim c1 As String = "scheme://user:" & Pwd & "@domain.com"
            Dim c2 As String = "scheme://user:@domain.com"
            Dim c3 As String = "scheme://user@domain.com:80"
            Dim c4 As String = "scheme://user@domain.com"
            Dim c5 As String = "scheme://domain.com/user:azerty123"
            Dim c6 As String = String.Format("scheme://user:{0}@domain.com", Pwd)
            Dim c7 As String = $"scheme://user:{Pwd}@domain.com"

            Dim e1 As String = "scheme://admin:admin@domain.com"    ' Compliant exception, user and password are the same
            Dim e2 As String = "scheme://abc:abc@domain.com"        ' Compliant exception, user and password are the same
            Dim e3 As String = "scheme://a%20;c:a%20;c@domain.com"  ' Compliant exception, user and password are the same
        End Sub

    End Class

    Public Class SqlConnection
        Implements IDisposable

        Public Sub New(ConnStr As String)
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
            Dim query2 As String = "password=hardcoded;user='" + user + "'" ' False Negative - Only LiteralExpressionSyntax nodes are covered
        End Sub

        Class Configuration
            Public Shared Property Password As String
        End Class
    End Class

End Namespace
