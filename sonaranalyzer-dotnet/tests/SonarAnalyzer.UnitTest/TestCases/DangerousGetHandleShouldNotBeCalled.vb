Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Runtime.InteropServices

Module Tests

  Sub Dangerous(fieldInfo As System.Reflection.FieldInfo)
    Dim handle As SafeHandle = CType(fieldInfo.GetValue(fieldInfo), SafeHandle)
    Dim dangerousHandle As IntPtr = handle.DangerousGetHandle() ' Noncompliant {{Refactor the code to remove this use of 'SafeHandle.DangerousGetHandle'.}}
'                                          ^^^^^^^^^^^^^^^^^^
    handle.DangerousGetHandle() ' Noncompliant
  End Sub

  Public Sub Test(bar As Bar)
    Dim dangerousHandle As IntPtr = bar.DangerousHandle() ' compliant
  End Sub

  Class Bar
    Sub Test()
      DangerousHandle()
    End Sub
    Function DangerousHandle() As IntPtr
    End Function
  End Class
End Module
