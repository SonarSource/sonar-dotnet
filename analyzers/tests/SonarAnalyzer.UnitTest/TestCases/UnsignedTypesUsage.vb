Imports System

Module Module1
    Sub Main()
        Dim foo1 As UShort   ' Noncompliant {{Change this unsigned type to 'Short'.}}
'                   ^^^^^^
        Dim foo2 As UInteger ' Noncompliant {{Change this unsigned type to 'Integer'.}}
        Dim foo3 As ULong    ' Noncompliant {{Change this unsigned type to 'Long'.}}
        Dim foo4 As System.UInt64 ' Noncompliant
'                   ^^^^^^^^^^^^^
        Dim foo5 As UInt64 ' Noncompliant
        Dim foo6 As UInt64() ' Noncompliant
'                   ^^^^^^
        Dim foo7 As UInt64? ' Noncompliant
'                   ^^^^^^
        Dim foo8 As Nullable(Of UInt64) ' Noncompliant
'                               ^^^^^^
    End Sub

    Sub Main2()
        Dim foo1 As Short
        Dim foo2 As Integer
        Dim foo3 As Long
        Dim foo4 As System.Int64
        Dim foo5 As Int64
        Dim foo6 As Int64()
        Dim foo7 As Int64?
        Dim foo8 As Nullable(Of Int64)
    End Sub
End Module
