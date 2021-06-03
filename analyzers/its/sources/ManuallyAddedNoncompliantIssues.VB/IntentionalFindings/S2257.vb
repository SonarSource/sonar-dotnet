' <Your-Product-Name>
' Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
'
' Please configure this header in your SonarCloud/SonarQube quality profile.
' You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.

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

    Public Sub Dispose() Implements IDisposable.Dispose
        Throw New NotImplementedException()
    End Sub

End Class

Public Class CustomAsymmetricAlgorithm  ' Noncompliant
    Inherits AsymmetricAlgorithm

End Class
