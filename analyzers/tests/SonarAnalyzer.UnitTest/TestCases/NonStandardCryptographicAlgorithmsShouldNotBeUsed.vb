Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Security.Cryptography

Public Class CustomHash  ' Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
'            ^^^^^^^^^^
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

Public class CustomCryptoTransform  ' Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
'            ^^^^^^^^^^^^^^^^^^^^^
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

Public Interface ICustomCryptoTransform  ' Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
'                ^^^^^^^^^^^^^^^^^^^^^^
    Inherits ICryptoTransform

End Interface

Public class CustomCryptoTransformWithInterface  ' Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
'            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    Implements ICustomCryptoTransform

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

Public class CustomDerivebytes  ' Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
'            ^^^^^^^^^^^^^^^^^
    Inherits DeriveBytes

    Public Overrides Sub Reset()
        Throw New NotImplementedException()
    End Sub

    Public Overrides Function GetBytes(cb As Integer) As Byte()
        Throw New NotImplementedException()
    End Function

End Class

Public class CustomSymmetricAlgorithm  ' Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
'            ^^^^^^^^^^^^^^^^^^^^^^^^
    Inherits SymmetricAlgorithm

    Public Overrides Sub GenerateKey()
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub GenerateIV()
        Throw New NotImplementedException()
    End Sub

    Public Overrides Function CreateEncryptor(rgbKey() As Byte, rgbIV() As Byte) As ICryptoTransform
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateDecryptor(rgbKey() As Byte, rgbIV() As Byte) As ICryptoTransform
        Throw New NotImplementedException()
    End Function

End Class

Public class CustomAsymmetricAlgorithm  ' Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
'            ^^^^^^^^^^^^^^^^^^^^^^^^^
    Inherits AsymmetricAlgorithm

End Class

Public class DerivedClassFromCustomAsymmetricAlgorithm  ' Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
'            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    Inherits CustomAsymmetricAlgorithm

End Class

Public class CustomAsymmetricKeyExchangeDeformatter  ' Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
'            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    Inherits AsymmetricKeyExchangeDeformatter
    Public Overrides Property Parameters As String
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    Public Overrides Sub SetKey(key As AsymmetricAlgorithm)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Function DecryptKeyExchange(rgb() As Byte) As Byte()
        Throw New NotImplementedException()
    End Function

End Class

Public Class CustomAsymmetricKeyExchangeFormatter  ' Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
'            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    Inherits AsymmetricKeyExchangeFormatter

    Public Overrides ReadOnly Property Parameters As String
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public Overrides Sub SetKey(key As AsymmetricAlgorithm)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Function CreateKeyExchange(data() As Byte) As Byte()
        Throw New NotImplementedException()
    End Function

    Public Overrides Function CreateKeyExchange(data() As Byte, symAlgType As Type) As Byte()
        Throw New NotImplementedException()
    End Function

End Class

Public class CustomAsymmetricSignatureDeformatter  ' Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
'            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    Inherits AsymmetricSignatureDeformatter

    Public Overrides Sub SetKey(key As AsymmetricAlgorithm)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub SetHashAlgorithm(strName As String)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Function VerifySignature(rgbHash() As Byte, rgbSignature() As Byte) As Boolean
        Throw New NotImplementedException()
    End Function

End Class

Public class CustomAsymmetricSignatureFormatter  ' Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
'            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    Inherits AsymmetricSignatureFormatter

    Public Overrides Sub SetKey(key As AsymmetricAlgorithm)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Sub SetHashAlgorithm(strName As String)
        Throw New NotImplementedException()
    End Sub

    Public Overrides Function CreateSignature(rgbHash() As Byte) As Byte()
        Throw New NotImplementedException()
    End Function
End Class

Public class CustomKeyedHashAlgorithm  ' Noncompliant {{Make sure using a non-standard cryptographic algorithm is safe here.}}
'            ^^^^^^^^^^^^^^^^^^^^^^^^
    Inherits KeyedHashAlgorithm

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

Public class ClassThatDoesNotInheritOrImplementAnything  ' Compliant

End Class

Public Interface InterfaceThatDoesNotInheritOrImplementAnything  ' Compliant

End Interface

Public Interface InterfaceThatDoesNotInheritCryptographic  ' Compliant
    Inherits IDisposable

End Interface

Public Class ClassThatDoesNotInheritCryptographic  ' Compliant
    Implements IDisposable

    Private disposedValue As Boolean

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
