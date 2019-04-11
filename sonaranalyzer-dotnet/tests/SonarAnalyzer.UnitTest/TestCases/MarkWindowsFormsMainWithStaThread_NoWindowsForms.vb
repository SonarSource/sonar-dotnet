Imports System
Imports System.Windows.Forms
Imports System.Threading
Imports System.Threading.Tasks

Namespace Tests.TestCases

  Class Program_00
    <Nonsense>
    Shared Sub Main()
    End Sub
  End Class

  Public Class Program_01
    Shared Sub Main()
      Dim winForm As Form = New Form
      Application.Run(winForm)
    End Sub
  End Class

  Public Class Program_02
    Public Shared Sub Main()
    End Sub
  End Class

  Class Program_03
    Public Shared Sub Main(args As String())
    End Sub
  End Class

  Class Program_04
    <MTAThread()> Shared Sub Main()
    End Sub
  End Class

  Class Program_05
    <MTAThread()> Shared Sub Main(args As String())
    End Sub
  End Class

  Class Program_06
    Sub Main(args As String())
    End Sub
  End Class

  Public Class Program_07
    <STAThread()> Shared Sub Main()
      Dim winForm As Form = New Form
      Application.Run(winForm)
    End Sub
  End Class

  Public Class Program_08
    <STAThread> Shared Sub Main(args As String())
      Dim winForm As Form = New Form
      Application.Run(winForm)
    End Sub
  End Class

  Class Program_09
    <STAThread("random", 1)>
    Shared Sub Main()
    End Sub
  End Class

  Class Program_11
    <MTAThread>
    Shared Sub Pain()
    End Sub
  End Class

  Class Program_12
    Public Shared Async Function Main() As Task
      Await Task.CompletedTask
    End Function
  End Class

End Namespace
