Public Class Sample
    Public Event Changed()

    Sub Go()
        RaiseEvent Changed()
    End Sub

    Sub Sample_Changed() Handles Me.Changed
    End Sub

End class
