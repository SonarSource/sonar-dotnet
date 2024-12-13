Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace Samples.VB

    Partial Public Class PartialClass
        <TestMethod>
        Public Sub InSecondFile()
        End Sub

        <TestMethod>
        Private Sub PartialMethod()
        End Sub
    End Class

End Namespace
