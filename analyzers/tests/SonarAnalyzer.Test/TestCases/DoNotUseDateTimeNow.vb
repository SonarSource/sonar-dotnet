Imports System
Imports System.Linq
Imports System.DateTime
Imports AliasedDateTime = System.DateTime

Class Test
    Private Sub TestCases()
        Dim currentTime = DateTime.Now                                                      ' Noncompliant {{Use UTC when recording DateTime instants}}
        '                 ^^^^^^^^^^^^
        currentTime = Date.Now                                                              ' Noncompliant
        currentTime = Date.Now                                                              ' Noncompliant
        currentTime = System.DateTime.Now                                                   ' Noncompliant
        currentTime = DateTime.Now                                                          ' Noncompliant
        currentTime = Now                                                                   ' FN
        currentTime = AliasedDateTime.Now                                                   ' Noncompliant

        Dim today = DateTime.Today                                                          ' Noncompliant - same as DateTime.Now, but the the time is set to 00:00:00

        Dim currentTimeWithOffset = DateTimeOffset.Now                                      ' Compliant - DateTimeOffset stores the time zone
        currentTimeWithOffset = DateTimeOffset.UtcNow

        currentTime = DateTimeOffset.Now.DateTime                                           ' Noncompliant - same as DateTime.Now
        currentTime = DateTimeOffset.UtcNow.DateTime                                        ' Compliant - same as DateTime.UtcNow

        Dim currentDate = DateTimeOffset.Now.Date                                           ' Noncompliant - same as DateTime.Now.Date
        currentDate = DateTimeOffset.UtcNow.Date                                            ' Compliant - same as DateTime.UtcNow.Date

        currentDate = DateTime.Now.AddDays(1)                                               ' Noncompliant
        currentDate = DateTime.Now.AddDays(1)                                               ' Noncompliant
        currentDate = DateTime.Now + TimeSpan.FromDays(1)                                   ' Noncompliant

        If DateTime.Now > currentTime Then                                                  ' Noncompliant
        End If

        Dim hours = Enumerable.Range(0, 10).Select(Function(x) DateTime.Now.AddHours(x))    ' Noncompliant

        Dim propertyName = NameOf(DateTime.Now)                                             ' Compliant
    End Sub
End Class

Class CustomTypeCalledDateTime
    Public Structure DateTime
        Public Shared ReadOnly Property Now As DateTime
            Get
                Return New DateTime()
            End Get
        End Property
    End Structure

    Private Sub New()
        Dim instant = DateTime.Now                                                          ' Compliant - this is not System.DateTime
    End Sub
End Class

Class FakeNameOf
    Private Function [nameof](o As Object) As String
        Return ""
    End Function

    Private Sub UsesFakeNameOfMethod()
        [nameof](Date.Now)                                                                  ' Noncompliant
    End Sub
End Class
