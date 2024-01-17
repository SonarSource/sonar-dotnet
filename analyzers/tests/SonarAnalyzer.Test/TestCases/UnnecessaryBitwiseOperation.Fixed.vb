Imports System
Imports System.Collections.Generic

Public Enum Types
    [Class] = 0
    Struct = 1
    [Public] = 2
    [Private] = 4
End Enum

Class UnnecessaryBitwiseOperation

    Public Sub Method()
        Dim Result As Integer
        Dim Zero As Integer = 0
        Dim One As Integer = 1
        Dim BitMask As Integer = &H10F

        Result = BitMask ' Fixed
        Result = BitMask   ' Fixed
        Result = BitMask  ' Fixed
        Result = BitMask  ' Fixed
        Result = BitMask   ' Fixed

        Result = BitMask And 1  ' Compliant
        Result = BitMask Or 1   ' Compliant
        Result = BitMask Xor 1  ' Compliant
        Result = BitMask Xor One    ' Compliant

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

        Dim Flags As Types = Types.Class Or Types.Private   ' Compliant even when class is zero
        Flags = Types.Class
        Flags = Flags Or Types.Private      ' Compliant, even when "flags" was initally zero
    End Sub

    Private Shared Function ReturnLong() As Long
        Return 1L
    End Function

    Private Shared Sub MyMethod(U As UInt64)
    End Sub

End Class

' https//github.com/SonarSource/sonar-dotnet/issues/4399
Public Class Repro_4399

    Public Sub BuildMask(DaysOfWeek As IEnumerable(Of DayOfWeek))
        Dim Value As Integer = 0
        For Each dow As DayOfWeek In DaysOfWeek
            Value = Value Or (1 << CInt(dow))   ' Compliant, value changes over iterations
        Next
    End Sub

    Public Sub Repro(Args() As Object)
        Dim Fail As Boolean = False
        For Each Arg As Object In Args
            Fail = Fail Or Not CheckArg(Arg)    ' Compliant, using short-circuit operator OrElse would change the logic.
        Next
    End Sub

    Private Function CheckArg(Arg As Object) As Boolean
    End Function

    Public Sub FindConstant_For_AssignedInsideLoop()
        Dim Value As Integer = 1
        Dim Result As Integer
        For v As Integer = 0 To 41
            Value = 0
            Result = v     ' Fixed
        Next
    End Sub

    Public Sub FindConstant_For_ReassignedToTheSameValue()
        Dim Value As Integer = 0
        Dim Result As Integer
        For v As Integer = 0 To 41
            Result = Value Or v     ' FN per rule description, but expected behavior for ConstantValueFinder. Variable "Value" is reassigned inside a Loop.
            Value = 0
        Next
    End Sub

    Public Sub FindConstant_For()
        Dim Value As Integer = 0
        Dim Unchanged As Integer = 0
        Dim Result As Integer
        For v As Integer = 0 To 41
            Result = Value Or v         ' Compliant, value changes over iterations
            Result = v     ' Fixed
            Value = 1
        Next
        Unchanged = 1
    End Sub

    Public Sub FindConstant_ForEach(Values() As Integer)
        Dim Value As Integer = 0
        Dim Unchanged As Integer = 0
        Dim Result As Integer
        For Each v As Integer In Values
            Result = Value Or v         ' Compliant, value changes over iterations
            Result = v     ' Fixed
            Value = 1
        Next
        Unchanged = 1
    End Sub

    Public Sub FindConstant_While(Values() As Integer)
        Dim Value As Integer = 0
        Dim Unchanged As Integer = 0
        Dim Result As Integer
        Dim Index As Integer
        While Index < Values.Length
            Dim v As Integer = Values(Index)
            Result = Value Or v         ' Compliant, value changes over iterations
            Result = v     ' Fixed
            Value = 1
            Index += 1
        End While
        Unchanged = 1
    End Sub

    Public Sub FindConstant_DoWhile(Values() As Integer)
        Dim Value As Integer = 0
        Dim Unchanged As Integer = 0
        Dim Result As Integer
        Dim Index As Integer
        Do While Index < Values.Length
            Dim v As Integer = Values(Index)
            Result = Value Or v         ' Compliant, value changes over iterations
            Result = v     ' Fixed
            Value = 1
            Index += 1
        Loop
        Unchanged = 1
    End Sub

    Public Sub FindConstant_DoUntil(Values() As Integer)
        Dim Value As Integer = 0
        Dim Unchanged As Integer = 0
        Dim Result As Integer
        Dim Index As Integer
        Do Until Index >= Values.Length
            Dim v As Integer = Values(Index)
            Result = Value Or v         ' Compliant, value changes over iterations
            Result = v     ' Fixed
            Value = 1
            Index += 1
        Loop
        Unchanged = 1
    End Sub

    Public Sub FindConstant_DoLoopWhile(Values() As Integer)
        Dim Value As Integer = 0
        Dim Unchanged As Integer = 0
        Dim Result As Integer
        Dim Index As Integer
        Do
            Dim v As Integer = Values(Index)
            Result = Value Or v         ' Compliant, value changes over iterations
            Result = v     ' Fixed
            Value = 1
            Index += 1
        Loop While Index < Values.Length
        Unchanged = 1
    End Sub

    Public Sub FindConstant_DoLoopUntil(Values() As Integer)
        Dim Value As Integer = 0
        Dim Unchanged As Integer = 0
        Dim Result As Integer
        Dim Index As Integer
        Do
            Dim v As Integer = Values(Index)
            Result = Value Or v         ' Compliant, value changes over iterations
            Result = v     ' Fixed
            Value = 1
            Index += 1
        Loop Until Index >= Values.Length
        Unchanged = 1
    End Sub

End Class

Public Class LocalFields

    Private Start As Integer = 0

    Public Property [End] As Integer = 0

    Public Sub UpdateStart(Val As Integer)
        Start = Val
    End Sub

    Public Overrides Function GetHashCode() As Integer
        Dim Value1 = Method() ^ [End]
        Dim Value2 = Start ^ [End]
        Dim Value3 = [End] ^ Start
        Return Method() ^ Start
    End Function

    Private Function Method() As Integer
        Return 42
    End Function

End Class
