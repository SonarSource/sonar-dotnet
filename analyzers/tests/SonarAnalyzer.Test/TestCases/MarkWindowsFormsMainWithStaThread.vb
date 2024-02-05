' Error [BC30738] 'Sub Main' is declared more than once in 'project0'
Imports System
Imports System.Windows.Forms
Imports System.Threading
Imports System.Threading.Tasks

Namespace Tests.TestCases

  Class Program_00
        ' Error@+1 [BC30002] Attribute doesn't exist
        <Nonsense>
        Shared Sub Main() ' Noncompliant
        End Sub
    End Class

  Public Class Program_01
    Shared Sub Main() ' Noncompliant {{Add the 'STAThread' attribute to this entry point.}}
'              ^^^^
    End Sub
  End Class

  Public Class Program_02
    Public Shared Sub Main() ' Noncompliant {{Add the 'STAThread' attribute to this entry point.}}
    End Sub
  End Class

  class LowerCaseProgram
    public shared sub main(args as string()) ' Noncompliant {{Add the 'STAThread' attribute to this entry point.}}
    end sub
  end class

  Class Program_04
    <MTAThread()> Shared Sub Main() ' Noncompliant {{Change the 'MTAThread' attribute of this entry point to 'STAThread'.}}
    End Sub
  End Class

  Class Program_05
    <MTAThread()> Shared Sub Main(args As String()) ' Noncompliant {{Change the 'MTAThread' attribute of this entry point to 'STAThread'.}}
    End Sub
  End Class

  Class Program_06
    Sub Main(args As String()) ' not static method
    End Sub
  End Class

  Public Class Program_07
    <STAThread()> Shared Sub Main()
    End Sub
  End Class

  Public Class Program_08
    <STAThread> Shared Sub Main(args As String())
    End Sub
  End Class

  Class Program_09
        ' Error@+1 [BC30057] Invalid code
        <STAThread("random", 1)>
        Shared Sub Main()
        End Sub
    End Class

  Class Program_10
    <MTAThread>
    Shared Sub Pain()
    End Sub
  End Class

  Class Program_11
    Public Shared Async Function Main() As Task
      Await Task.CompletedTask
    End Function
  End Class

  Class Program_12
    <STAThread>
    Public Shared Async Function Main() As Task
        Await Task.CompletedTask
    End Function
  End Class

  Class Program_13
    <MTAThread>
    Public Shared Async Function Main() As Task
        Await Task.CompletedTask
    End Function
  End Class

End Namespace
