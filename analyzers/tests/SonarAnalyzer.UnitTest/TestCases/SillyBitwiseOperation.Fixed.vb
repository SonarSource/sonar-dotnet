Class SillyBitwiseOperation

    Public Sub Method()
        Dim Result As Integer
        Dim BitMask As Integer = &H10F

        Result = BitMask ' Fixed
        Result = BitMask   ' Fixed
        Result = BitMask  ' Fixed
        Result = BitMask  ' Fixed {{Remove this silly bit operation.}}

        Result = BitMask And 1  ' Compliant
        Result = BitMask Or 1   ' compliant
        Result = BitMask Xor 1  ' Compliant

        Dim BitMaskLong As Long = &H10F
        Dim ResultLong As Long
        ResultLong = BitMaskLong ' Fixed
        ResultLong = BitMaskLong And 0L     ' Compliant
        ResultLong = BitMaskLong     ' Fixed
        ResultLong = BitMaskLong    ' Fixed
        ResultLong = BitMaskLong And ReturnLong() ' Compliant
        ResultLong = BitMaskLong And &HF   ' Compliant

        Dim ResultULong As ULong = 1UL     ' Fixed
        ResultULong = 1UL Or 18446744073709551615UL ' Compliant

        MyMethod(1UL) ' Fixed

        If True AndAlso True Then Return
        If True OrElse True Then Return
    End Sub

    Private Shared Function ReturnLong() As Long
        Return 1L
    End Function

    Private Shared Sub MyMethod(U As UInt64)
    End Sub

End Class
