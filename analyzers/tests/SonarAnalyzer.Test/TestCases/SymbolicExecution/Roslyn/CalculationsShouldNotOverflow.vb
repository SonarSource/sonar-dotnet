
Imports System

Public Class Sample

    Private __ As Object    'Dispose for the syntax below

    Public Sub Upcast()
        Dim sb As SByte = SByte.MaxValue
        __ = sb + 1             ' Compliant, upcast to int
        sb = SByte.MinValue
        __ = sb - 1             ' Compliant, upcast to int

        Dim b As Byte = Byte.MaxValue
        __ = b + 1              ' Compliant, upcast to int
        b = Byte.MinValue
        __ = b - 1              ' Compliant, upcast to int

        Dim i16 As Short = Short.MaxValue
        __ = i16 + 1            ' Compliant, upcast to int
        i16 = Short.MinValue
        __ = i16 - 1            ' Compliant, upcast to int

        Dim ui16 As UShort = UShort.MaxValue
        __ = ui16 + 1           ' Compliant, upcast to int
        ui16 = UShort.MinValue
        __ = ui16 - 1           ' Compliant, upcast to int

        Dim i As Integer = Integer.MaxValue
        __ = i + 1              ' Noncompliant
        i = Integer.MinValue
        __ = i - 1              ' Noncompliant

        Dim ui As UInteger = UInteger.MaxValue
        __ = ui + 1UI           ' Noncompliant
        __ = ui + 1             ' Compliant, because it's upcasted to Long
        ui = UInteger.MinValue
        __ = ui - 1UI           ' Noncompliant
        __ = ui - 1             ' Compliant, because it's upcasted to Long

        Dim i64 As Long = Long.MaxValue
        __ = i64 + 1            ' Noncompliant
        i64 = Long.MinValue
        __ = i64 - 1            ' Noncompliant

        Dim ui64 As ULong = ULong.MaxValue
        __ = ui64 + 1UI         ' Noncompliant
        __ = ui64 + 1           ' Compliant, because it's upcasted to Decimal
        ui64 = ULong.MinValue
        __ = ui64 - 1UI         ' Noncompliant
        __ = ui64 - 1           ' Compliant, because it's upcasted to Decimal
    End Sub

    Public Sub BasicOperators()
        Dim i As Integer = 2147483600
        __ = i + 100            ' Noncompliant {{This calculation is guaranteed to overflow the maximum value of '2147483647'.}}
        '    ^^^^^^^

        i = -2147483600
        __ = i - 100            ' Noncompliant {{This calculation is guaranteed to underflow the minimum value of '-2147483648'.}}

        i = 2147483600
        __ = i * 100            ' Noncompliant

        Dim j As Integer = 10
        i = 2147483600 \ j
        __ = i * 100            ' FN

        __ = 2147483600 << 16   ' Compliant
        __ = -2147483600 << 16  ' Compliant

        i = 2 And j
        __ = i * 2147483600     ' Noncompliant

        i = 2 Or j
        __ = i * 2147483600     ' Noncompliant

        i = 2 Xor j
        __ = i * 2147483600     ' Noncompliant

        i = 2 Mod j
        __ = i * 2147483600     ' Noncompliant

        Dim Result = Integer.MaxValue ^ 2  ' Compliant, result is Double
    End Sub

    Public Sub AssignmentOperators()
        Dim i As Integer = 2147483600
        i += 100                ' Noncompliant

        i = -2147483600
        i -= 100                ' Noncompliant

        i = 2147483600
        i *= 100                ' Noncompliant

        Dim j As Integer = 10
        i = 2147483600
        i \= j
        __ = i * 100            ' FN

        i = 2147483600
        i <<= 1                 ' Compliant

        i = -2147483600
        i <<= 1                 ' Compliant
    End Sub

    Public Sub Ranges(i As Integer)
        If i > 2147483600 Then __ = i + 100     ' Noncompliant {{This calculation is guaranteed to overflow the maximum value of '2147483647'.}}
        If i < 2147483600 Then __ = i + 100     ' Noncompliant {{This calculation is likely to overflow the maximum value of '2147483647'.}}
        If i > -2147483600 Then __ = i - 100    ' Noncompliant {{This calculation is likely to underflow the minimum value of '-2147483648'.}}
        If i < -2147483600 Then __ = i - 100    ' Noncompliant {{This calculation is guaranteed to underflow the minimum value of '-2147483648'.}}
    End Sub

    Public Sub Branching(i As Integer)
        If (i <= 2147483547) Then
            __ = i + 100        ' Compliant
        Else
            __ = i + 100        ' Noncompliant
        End If
        For i = 0 To 2147483547
            __ = i + 100        ' Compliant
        Next
        For i = 0 To 2147483547
            __ = i + 101        ' FN
        Next
        For i = 2147483546 To 2147483547
            __ = i + 100        ' Compliant
        Next
        For i = 2147483546 To 2147483547
            __ = i + 101        ' Noncompliant
        Next
    End Sub

    Public Sub Branching2(i As Integer)
        Select Case i
            Case 2147483547
                __ = i + 100    ' Compliant
            Case 2147483600
                __ = i + 100    ' Noncompliant
            Case Else
                __ = i + 100    ' Compliant
        End Select
    End Sub

    Public Sub Lambda()
        Dim a As Action = Sub()
                              Dim i As Integer = -2147483600
                              i -= 100           ' Noncompliant
                          End Sub
        Dim b As Func(Of Integer) = Function()
                                        Dim i As Integer = -2147483600
                                        i -= 100           ' Noncompliant
                                        Return i
                                    End Function
    End Sub

    Public Function DontRaiseOnUnknownValues(i As Integer) As Integer
        __ = i + 100
        __ = DontRaiseOnUnknownValues(i) + 100
        Return i
    End Function

    Public Overrides Function GetHashCode() As Integer
        Dim i As Integer = Integer.MaxValue
        Return i + 1    ' Noncompliant, we want to raise here for VB because it would throw an exception without masking
    End Function

End Class

Public Class Properties

    Public Property GetSet As Integer
        Get
            Dim i As Integer = 2147483600
            i += 100           ' Noncompliant
            Return i
        End Get
        Set(value As Integer)
            Dim i As Integer = 2147483600
            i += 100           ' Noncompliant
        End Set
    End Property

    Public Sub Untracked(o As Properties)
        If o.GetSet = Integer.MaxValue Then o.GetSet += 1         ' Compliant
        If o.GetSet = Integer.MinValue Then o.GetSet -= 1         ' Compliant
    End Sub

End Class

Public Class DotnetOverflow

    Public Function Overflow2() As Integer
        Dim i As Integer = 1834567890
        i += i                              ' Noncompliant
        Return i
    End Function

    Public Function Overflow3() As Integer
        Dim i As Integer = 1834567890
        Dim j As Integer = i + i            ' Noncompliant
        Return j
    End Function

    Public Function Overflow4() As Integer
        Dim i As Integer = -1834567890
        Dim j As Integer = 1834567890
        Dim k As Integer = i - j            ' Noncompliant
        Return k
    End Function

    Public Function Overflow5(i As Integer) As Integer
        If i > 1834567890 Then
            Return i + i                    ' Noncompliant
        End If
        Return 0
    End Function

End Class
