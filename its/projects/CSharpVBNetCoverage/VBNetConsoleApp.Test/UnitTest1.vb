Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace VBNetConsoleApp.Test
    <TestClass>
    Public Class UnitTest1
        <TestMethod>
        Sub TestSub()
            Assert.AreEqual(10, Module1.Passthrough(10))
        End Sub
    End Class
End Namespace

