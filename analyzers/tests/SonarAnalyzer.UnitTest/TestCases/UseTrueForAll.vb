Imports System.Text

Public Class Program

    Public Sub Test()
        Dim sb As StringBuilder = New StringBuilder()
        sb.Append("").Append("").ToString().ToLower().ToUpperInvariant() 'Noncompliant
        sb.Append("").Append("").ToString().ToLower()?.ToUpperInvariant() 'Noncompliant
        sb.Append("").Append("").ToString()?.ToLower().ToUpperInvariant() 'Noncompliant
        sb.Append("").Append("").ToString()?.ToLower()?.ToUpperInvariant() 'Noncompliant
        sb.Append("").Append("")?.ToString().ToLower().ToUpperInvariant() 'Noncompliant
        sb.Append("").Append("")?.ToString().ToLower()?.ToUpperInvariant() 'Noncompliant
        sb.Append("").Append("")?.ToString()?.ToLower().ToUpperInvariant() 'Noncompliant
        sb.Append("").Append("")?.ToString()?.ToLower()?.ToUpperInvariant() 'Noncompliant
        sb.Append("")?.Append("").ToString().ToLower().ToUpperInvariant() 'Noncompliant
        sb.Append("")?.Append("").ToString().ToLower()?.ToUpperInvariant() 'Noncompliant
        sb.Append("")?.Append("").ToString()?.ToLower().ToUpperInvariant() 'Noncompliant
        sb.Append("")?.Append("").ToString()?.ToLower()?.ToUpperInvariant() 'Noncompliant
        sb.Append("")?.Append("")?.ToString().ToLower().ToUpperInvariant() 'Noncompliant
        sb.Append("")?.Append("")?.ToString().ToLower()?.ToUpperInvariant() 'Noncompliant
        sb.Append("")?.Append("")?.ToString()?.ToLower().ToUpperInvariant() 'Noncompliant
        sb.Append("")?.Append("")?.ToString()?.ToLower()?.ToUpperInvariant() 'Noncompliant
    End Sub
End Class
