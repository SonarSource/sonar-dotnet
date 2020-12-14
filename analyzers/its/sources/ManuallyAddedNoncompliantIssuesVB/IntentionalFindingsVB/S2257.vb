' SonarQube, open source software quality management tool.
' Copyright (C) 2008-2020 SonarSource
' mailto:contact AT sonarsource DOT com
'
' SonarQube is free software; you can redistribute it and/or
' modify it under the terms of the GNU Lesser General Public
' License as published by the Free Software Foundation; either
' version 3 of the License, or (at your option) any later version.
'
' SonarQube is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
' Lesser General Public License for more details.
'
' You should have received a copy of the GNU Lesser General Public License
' along with this program; if not, write to the Free Software Foundation,
' Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

Imports System.Security.Cryptography

Public Class CustomHash  ' Noncompliant (S2257)
    Inherits HashAlgorithm

    Public Overrides Sub Initialize()
        Throw New NotImplementedException()
    End Sub

    Protected Overrides Sub HashCore(array() As Byte, ibStart As Integer, cbSize As Integer)
        Throw New NotImplementedException()
    End Sub

    Protected Overrides Function HashFinal() As Byte()
        Throw New NotImplementedException()
    End Function

End Class

Public Interface ICustomCryptoTransform  ' Noncompliant
    Inherits ICryptoTransform

End Interface

Public class CustomCryptoTransform  ' Noncompliant
    Implements ICryptoTransform

    Private disposedValue As Boolean

    Public ReadOnly Property InputBlockSize As Integer Implements ICryptoTransform.InputBlockSize
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public ReadOnly Property OutputBlockSize As Integer Implements ICryptoTransform.OutputBlockSize
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public ReadOnly Property CanTransformMultipleBlocks As Boolean Implements ICryptoTransform.CanTransformMultipleBlocks
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public ReadOnly Property CanReuseTransform As Boolean Implements ICryptoTransform.CanReuseTransform
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public Function TransformBlock(inputBuffer() As Byte, inputOffset As Integer, inputCount As Integer, outputBuffer() As Byte, outputOffset As Integer) As Integer Implements ICryptoTransform.TransformBlock
        Throw New NotImplementedException()
    End Function

    Public Function TransformFinalBlock(inputBuffer() As Byte, inputOffset As Integer, inputCount As Integer) As Byte() Implements ICryptoTransform.TransformFinalBlock
        Throw New NotImplementedException()
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects)
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
            ' TODO: set large fields to null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
    ' Protected Overrides Sub Finalize()
    '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

End Class

Public Class CustomAsymmetricAlgorithm  ' Noncompliant
    Inherits AsymmetricAlgorithm

End Class
