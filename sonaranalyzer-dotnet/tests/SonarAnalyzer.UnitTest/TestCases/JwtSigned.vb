Imports System
Imports JWT
Imports JWT.Algorithms
Imports JWT.Builder
Imports System.Collections.Generic

Namespace Tests.Diagnostics

    Class Program

        Protected Const Secret As String = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk"
        Protected Const InvalidToken As String = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJmb28iOiJmYWtlYmFyIiwiaWF0IjoxNTc1NjQ0NTc3fQ.pcX_7snpSGf01uBfaM8XPkbgdhs1gq9JcYRCQvZrJyk"

        Private InvalidParts As JwtParts

        ' Encoding with JWT.NET Is safe

        Sub DecodingWithDecoder(Decoder As JwtDecoder)
            Dim Decoded1 As String = Decoder.Decode(InvalidToken, Secret, True) ' Compliant
            Dim Decoded2 As String = Decoder.Decode(InvalidToken, Secret, False) ' Noncompliant {{Use only strong cipher algorithms when verifying the signature of this JWT.}}

            Dim Decoded3 As String = Decoder.Decode(InvalidToken, Secret, verify:=True) ' Compliant
            Dim Decoded4 As String = Decoder.Decode(InvalidToken, Secret, verify:=False) ' Noncompliant

            Dim Decoded5 As String = Decoder.Decode(InvalidToken, Secret, verify:=True) ' Compliant
            Dim Decoded6 As String = Decoder.Decode(InvalidToken, Secret, verify:=False) ' Noncompliant

            Dim Decoded7 As String = Decoder.Decode(InvalidToken, verify:=True, key:=Secret) ' Compliant
            Dim Decoded8 As String = Decoder.Decode(InvalidToken, verify:=False, key:=Secret) ' Noncompliant

            Dim Decoded9 As String = Decoder.Decode(InvalidToken, verify:=True, key:={42}) ' Compliant
            Dim Decoded10 As String = Decoder.Decode(InvalidToken, verify:=False, key:={42}) ' Noncompliant

            Dim Decoded11 As String = Decoder.Decode(InvalidToken) ' Noncompliant
            Dim Decoded12 As String = Decoder.Decode(InvalidParts) ' Noncompliant

            Dim Decoded21 As Object = Decoder.DecodeToObject(InvalidToken, Secret, True) ' Compliant
            Dim Decoded22 As Object = Decoder.DecodeToObject(InvalidToken, Secret, False) ' Noncompliant

            Dim Decoded31 As UserInfo = Decoder.DecodeToObject(Of UserInfo)(InvalidToken, Secret, True) ' Compliant
            Dim Decoded32 As UserInfo = Decoder.DecodeToObject(Of UserInfo)(InvalidToken, Secret, False) ' Noncompliant
        End Sub

        Sub DecodingWithCustomDecoder(Decoder As CustomDecoder)
            Dim Decoded1 As String = Decoder.Decode(InvalidToken, Secret, True) ' Compliant
            Dim Decoded2 As String = Decoder.Decode(InvalidToken, Secret, False) ' Noncompliant {{Use only strong cipher algorithms when verifying the signature of this JWT.}}

            Dim Decoded3 As String = Decoder.Decode(InvalidToken, Secret, verify:=True) ' Compliant
            Dim Decoded4 As String = Decoder.Decode(InvalidToken, Secret, verify:=False) ' Noncompliant

            Dim Decoded5 As String = Decoder.Decode(InvalidToken, Secret, verify:=True) ' Compliant
            Dim Decoded6 As String = Decoder.Decode(InvalidToken, Secret, verify:=False) ' Noncompliant

            Dim Decoded7 As String = Decoder.Decode(InvalidToken, verify:=True, key:=Secret) ' Compliant
            Dim Decoded8 As String = Decoder.Decode(InvalidToken, verify:=False, key:=Secret) ' Noncompliant

            Dim Decoded9 As String = Decoder.Decode(InvalidToken, verify:=True, key:={42}) ' Compliant
            Dim Decoded10 As String = Decoder.Decode(InvalidToken, verify:=False, key:={42}) ' Noncompliant

            Dim Decoded11 As String = Decoder.Decode(InvalidToken) ' Noncompliant
            Dim Decoded12 As String = Decoder.Decode(InvalidParts) ' Noncompliant

            Dim Decoded21 As Object = Decoder.DecodeToObject(InvalidToken, Secret, True) ' Compliant
            Dim Decoded22 As Object = Decoder.DecodeToObject(InvalidToken, Secret, False) ' Noncompliant

            Dim Decoded31 As UserInfo = Decoder.DecodeToObject(Of UserInfo)(InvalidToken, Secret, True) ' Compliant
            Dim Decoded32 As UserInfo = Decoder.DecodeToObject(Of UserInfo)(InvalidToken, Secret, False) ' Noncompliant
        End Sub

        Sub DecodingWithBuilder()
            Dim Decoded1 As String = New JwtBuilder().
                WithSecret(Secret).
                Decode(InvalidToken) ' Noncompliant {{Use only strong cipher algorithms when verifying the signature of this JWT.}}

            Dim Decoded2 As String = New JwtBuilder().
                WithSecret(Secret).
                MustVerifySignature().
                Decode(InvalidToken) ' Compliant

            Dim builder1 As JwtBuilder = New JwtBuilder().WithSecret(Secret)
            builder1.Decode(InvalidToken) ' Noncompliant

            Dim builder2 As JwtBuilder = builder1.MustVerifySignature()
            builder2.Decode(InvalidToken) ' Compliant

            Dim builder3 As JwtBuilder = New JwtBuilder().WithSecret(Secret).MustVerifySignature()
            builder3.Decode(InvalidToken) ' Compliant

            Dim Decoded11 As String = New JwtBuilder().
                WithSecret(Secret).
                WithVerifySignature(True).
                MustVerifySignature().
                DoNotVerifySignature().
                Decode(InvalidToken) ' Noncompliant

            Dim Decoded12 As String = New JwtBuilder().
                WithSecret(Secret).
                WithVerifySignature(False).
                DoNotVerifySignature().
                MustVerifySignature().
                Decode(InvalidToken) ' Compliant

            Dim Decoded21 As String = New JwtBuilder().
                WithSecret(Secret).
                DoNotVerifySignature().
                WithVerifySignature(False).
                WithVerifySignature(True).
                Decode(InvalidToken) ' Compliant

            Dim Decoded31 As String = New JwtBuilder().
                WithSecret(Secret).
                MustVerifySignature().
                WithVerifySignature(True).
                WithVerifySignature(False).
                Decode(InvalidToken) ' Noncompliant
        End Sub

    End Class

    Public Class UserInfo

        Public Property Name As String

    End Class

    Public Class CustomDecoder
        Implements IJwtDecoder

        Public Function Decode(jwt As JwtParts) As String Implements IJwtDecoder.Decode
        End Function

        Public Function Decode(token As String) As String Implements IJwtDecoder.Decode
        End Function

        Public Function Decode(token As String, key As String, verify As Boolean) As String Implements IJwtDecoder.Decode
        End Function

        Public Function Decode(token As String, keys() As String, verify As Boolean) As String Implements IJwtDecoder.Decode
        End Function

        Public Function Decode(token As String, key() As Byte, verify As Boolean) As String Implements IJwtDecoder.Decode
        End Function

        Public Function Decode(token As String, keys As Byte()(), verify As Boolean) As String Implements IJwtDecoder.Decode
        End Function

        Public Function DecodeToObject(token As String) As IDictionary(Of String, Object) Implements IJwtDecoder.DecodeToObject
        End Function

        Public Function DecodeToObject(token As String, key As String, verify As Boolean) As IDictionary(Of String, Object) Implements IJwtDecoder.DecodeToObject
        End Function

        Public Function DecodeToObject(token As String, keys() As String, verify As Boolean) As IDictionary(Of String, Object) Implements IJwtDecoder.DecodeToObject
        End Function

        Public Function DecodeToObject(token As String, key() As Byte, verify As Boolean) As IDictionary(Of String, Object) Implements IJwtDecoder.DecodeToObject
        End Function

        Public Function DecodeToObject(token As String, keys As Byte()(), verify As Boolean) As IDictionary(Of String, Object) Implements IJwtDecoder.DecodeToObject
        End Function

        Public Function DecodeToObject(Of T)(token As String) As T Implements IJwtDecoder.DecodeToObject
        End Function

        Public Function DecodeToObject(Of T)(token As String, key As String, verify As Boolean) As T Implements IJwtDecoder.DecodeToObject
        End Function

        Public Function DecodeToObject(Of T)(token As String, keys() As String, verify As Boolean) As T Implements IJwtDecoder.DecodeToObject
        End Function

        Public Function DecodeToObject(Of T)(token As String, key() As Byte, verify As Boolean) As T Implements IJwtDecoder.DecodeToObject
        End Function

        Public Function DecodeToObject(Of T)(token As String, keys As Byte()(), verify As Boolean) As T Implements IJwtDecoder.DecodeToObject
        End Function

    End Class

End Namespace
