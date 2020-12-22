Imports System
Imports System.Security.Cryptography

Namespace NS

    Public Class TestClass

        ' RSPEC 4790 https://jira.sonarsource.com/browse/RSPEC-4790
        Public Sub ComputeHash()

            ' Review all instantiations of classes that inherit from HashAlgorithm, for example:
            Dim hashAlgo As HashAlgorithm = HashAlgorithm.Create()
'                                           ^^^^^^^^^^^^^^^^^^^^^^    {{Make sure this weak hash algorithm is not used in a sensitive context here.}}
            Dim hashAlgo2 As HashAlgorithm = HashAlgorithm.Create("SHA1")
'                                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^    {{Make sure this weak hash algorithm is not used in a sensitive context here.}}

            Dim sha As SHA1 = New SHA1CryptoServiceProvider()
'                             ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^    {{Make sure this weak hash algorithm is not used in a sensitive context here.}}

            Dim md5 As MD5 = New MD5CryptoServiceProvider()
'                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^    {{Make sure this weak hash algorithm is not used in a sensitive context here.}}

            ' ...
        End Sub

        Public Sub AdditionalTests(sha As SHA1CryptoServiceProvider)
            Dim myHash = New MyHashAlgorithm()
'                        ^^^^^^^^^^^^^^^^^^^^^    {{Make sure this weak hash algorithm is not used in a sensitive context here.}}
            myHash = New MyHashAlgorithm(123)       ' Noncompliant

            myHash = MyHashAlgorithm.Create()       ' Noncompliant
'                    ^^^^^^^^^^^^^^^^^^^^^^^^    {{Make sure this weak hash algorithm is not used in a sensitive context here.}}
            myHash = MyHashAlgorithm.Create(42)     ' Noncompliant

            myHash = MyHashAlgorithm.CreateHash()  ' compliant - method name is not Create
            myHash = MyHashAlgorithm.DoCreate()    ' compliant - method name is not Create


            '  Other methods are not checked
            Dim hash = sha.ComputeHash(CType(Nothing, Byte()))
            hash = sha.Hash
            Dim canReuse = sha.CanReuseTransform
            sha.Clear()

        End Sub

    End Class

    Public Class MyHashAlgorithm
        Inherits System.Security.Cryptography.HashAlgorithm
'                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        Implements System.IDisposable

        Public Sub New()
        End Sub
        Public Sub New(Data As Integer)
        End Sub

        Public Shared Function Create() As MyHashAlgorithm
            Return Nothing
        End Function
        Public Shared Function Create(data As Integer) As MyHashAlgorithm
            Return Nothing
        End Function

        Public Shared Function CreateHash() As MyHashAlgorithm
            Return Nothing
        End Function

        Public Shared Function DoCreate() As MyHashAlgorithm
            Return Nothing
        End Function


#Region "Abstract method implementations"

        Public Overrides Sub Initialize()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub HashCore(array() As Byte, ibStart As Integer, cbSize As Integer)
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Function HashFinal() As Byte()
            Throw New NotImplementedException()
        End Function

#End Region

        Public Sub Dispose()
            'no-op
        End Sub

    End Class


    ' Check reporting on partial classes. Should only report once.
    Public Interface IMarker
    End Interface

    Public Interface IMarker2
    End Interface


    Partial Public Class PartialClassAlgorithm
        Inherits NS.MyHashAlgorithm
'                ^^^^^^^^^^^^^^^^^^
        Implements IMarker
    End Class

    Partial Public Class PartialClassAlgorithm
        Implements IMarker2
    End Class

End Namespace
