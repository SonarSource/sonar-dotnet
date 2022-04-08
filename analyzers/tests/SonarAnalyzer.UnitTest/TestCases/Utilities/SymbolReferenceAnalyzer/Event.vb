Public Class Sample

    Public Event Changed()

    Sub Go()
        AddHandler Changed, AddressOf Sample_Changed

        RaiseEvent Changed()
    End Sub

    Sub Sample_Changed() Handles Me.Changed
    End Sub

End class
