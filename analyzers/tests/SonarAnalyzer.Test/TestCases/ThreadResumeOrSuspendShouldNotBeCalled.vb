Imports System.Runtime.InteropServices
Imports System.Threading

Class Program

  Public Sub Foo()
    Thread.CurrentThread.Suspend() ' Noncompliant {{Refactor the code to remove this use of 'Thread.Suspend'.}}
    Thread.CurrentThread.[Resume]() ' Noncompliant {{Refactor the code to remove this use of 'Thread.Resume'.}}
    Dim thread1 = Thread.CurrentThread
    thread1.Suspend() ' Noncompliant
    thread1?.Resume() ' Noncompliant
  End Sub

  Public Sub Bar()
    Dim t As MyThread = new MyThread
    t.Resume()
    t.Suspend()
  End Sub

  Class MyThread
    Public Sub Suspend()
    End Sub
    Public Sub [Resume]()
    End Sub
  End Class

End Class
