Public Class DuplicatedTestClass2

    Public Property DefinitelyNotDuplicated As String

    Public Property SomeDuplicatedProperty As String

    Public Property AnotherDuplicatedProperty As String

    Public Sub UniqueMethod(Parameter As String)
        Console.WriteLine(Parameter)
    End Sub

    Public Function FirstDuplicatedMethod(A As Int32, B As Int32) As Int32
        If A < B Then
            Return A
        ElseIf B < A
            Return B
        Else
            Return A + B
        End If
    End Function

     Public Function SecondDuplicatedMethod(A As Int32, B As Int32) As Int32
        If A < B Then
            Return A
        ElseIf B < A
            Return B
        Else
            Return A + B
        End If
    End Function

    Public Sub ThisMethodIsNotDuplicated()
        Console.WriteLine("This is not a duplicated method.")
    End Sub

    Public Sub YetAnotherNotDuplicatedMethod()
        Console.WriteLine("This is yet not another duplicated method.")
    End Sub

    Public Sub ThirdDuplicatedMethod()
        Console.WriteLine(1)
        Console.WriteLine(2)
        Console.WriteLine(3)
        Console.WriteLine(4)
        Console.WriteLine(5)
    End Sub

    Public Sub YetAnotherDuplicatedMethod(A As String)
        If String.IsNullOrWhiteSpace(A) Then
            A = "somestring"
        End If

        Console.WriteLine(A)
    End Sub

End Class
