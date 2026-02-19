Imports System
Imports System.Security

Namespace Tests.Diagnostics
    Class Issue_7323
        Public Sub Noncompliant()
            Using securePwd As SecureString = New SecureString()
                For i As Integer = 0 To "AP@ssw0rd".Length - 1
                    securePwd.AppendChar("AP@ssw0rd"(i)) ' Noncompliant {{Please review this hard-coded password.}}
                Next
            End Using
        End Sub

        Public Sub Compliant(ByVal password As String)
            Using securePwd As SecureString = New SecureString()
                For i As Integer = 0 To password.Length - 1
                    securePwd.AppendChar(password(i))
                Next
            End Using
        End Sub
    End Class

    Class Tests
        Public Sub Characters()
            Using securePwd = New SecureString()
                securePwd.AppendChar("P"c) ' Noncompliant
                securePwd.AppendChar("a"c) ' Noncompliant
                securePwd.AppendChar("s"c) ' Noncompliant
                securePwd.AppendChar("s"c) ' Noncompliant
                Dim w = "w"c
                securePwd.AppendChar(w)    ' FN
            End Using
        End Sub

        Public Sub Constant()
            Const keyword As String = "AP@ssw0rd"
            Using securePwd As SecureString = New SecureString()

                For i As Integer = 0 To keyword.Length - 1
                    securePwd.AppendChar(keyword(i)) ' Noncompliant
                Next
            End Using
        End Sub

        Public Sub ForEachOverString()
            Using securePwd As SecureString = New SecureString()
                For Each c In "AP@ssw0rd"
                    securePwd.AppendChar(c) ' Noncompliant
                Next
            End Using
        End Sub

        Public Sub FromUnmodifiedVariable_ForLoop()
            Dim keyword = "AP@ssw0rd"
            Using securePwd As SecureString = New SecureString()

                For i As Integer = 0 To keyword.Length - 1
                    securePwd.AppendChar(keyword(i)) ' Noncompliant
                Next
            End Using
        End Sub

        Public Sub FromUnmodifiedVariable_ForEachLoop()
            Dim keyword = "AP@ssw0rd"
            Using securePwd As SecureString = New SecureString()

                For Each c In keyword
                    securePwd.AppendChar(c) ' Noncompliant
                Next
            End Using
        End Sub

        Public Sub FromBytes()
            Dim bytes = New Byte() {80, 97, 115, 115, 119, 111, 114, 100}
            Using securePwd As SecureString = New SecureString()
                For i As Integer = 0 To bytes.Length - 1
                    securePwd.AppendChar(Convert.ToChar(bytes(i))) ' FN.
                Next
            End Using
        End Sub

        Public Sub FromCharArray()
            Dim chars = New Char() {"P"c, "a"c, "s"c, "s"c, "w"c, "o"c, "r"c, "d"c}
            Using securePwd As SecureString = New SecureString()
                For i As Integer = 0 To chars.Length - 1
                    securePwd.AppendChar(chars(i)) ' FN.
                Next
            End Using
        End Sub

        Public Sub FromExpression()
            Dim chars = New Char() { "P"c, "a"c, "s"c, "s"c }
            Using securePwd As SecureString = New SecureString()
                For i As Integer = 0 To chars.Length - 1
                    securePwd.AppendChar("a"c + chars(i)) ' FN.
                Next
            End Using
        End Sub
    End Class
End Namespace
