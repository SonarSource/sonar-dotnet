Imports System
Imports JWT
Imports JWT.Algorithms
Imports JWT.Builder

Namespace Tests.Diagnostics

    Class Program

        Protected Const Secret As String = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk"
        Protected Const InvalidToken As String = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJmb28iOiJmYWtlYmFyIiwiaWF0IjoxNTc1NjQ0NTc3fQ.pcX_7snpSGf01uBfaM8XPkbgdhs1gq9JcYRCQvZrJyk"

        Private Algorithm As New HMACSHA256Algorithm()

        ' Encoding with JWT.NET Is safe

        Sub DecodingWithDecoder()
            Dim Decoder As New JwtDecoder(Nothing, Nothing, Nothing, Algorithm)

            Dim Decoded1 As String = Decoder.Decode(InvalidToken, Secret, True) ' Compliant
            Dim decoded2 As String = Decoder.Decode(InvalidToken, Secret, False) ' Noncompliant {{Use only strong cipher algorithms When verifying the signature Of this JWT.}}

            Dim decoded3 As String = Decoder.Decode(InvalidToken, Secret, verify:=True) ' Compliant
            Dim decoded4 As String = Decoder.Decode(InvalidToken, Secret, verify:=False) ' Noncompliant

            Dim decoded5 As String = Decoder.Decode(InvalidToken, Secret, verify:=True) ' Compliant
            Dim decoded6 As String = Decoder.Decode(InvalidToken, Secret, verify:=False) ' Noncompliant

            Dim decoded7 As String = Decoder.Decode(InvalidToken, verify:=True, key:=Secret) ' Compliant
            Dim decoded8 As String = Decoder.Decode(InvalidToken, verify:=False, key:=Secret) ' Noncompliant

            Dim decoded9 As String = Decoder.Decode(InvalidToken, verify:=True, key:={42}) ' Compliant
            Dim decoded10 As String = Decoder.Decode(InvalidToken, verify:=False, key:={42}) ' Noncompliant

            Dim decoded11 As String = Decoder.Decode(InvalidToken) ' Noncompliant
        End Sub

        Sub DecodingWithBuilder()
            Dim decoded1 As String = New JwtBuilder().
                WithSecret(Secret).
                Decode(InvalidToken) ' Noncompliant {{Use only strong cipher algorithms When verifying the signature Of this JWT.}}

            Dim decoded2 As String = New JwtBuilder().
                WithSecret(Secret).
                MustVerifySignature().
                Decode(InvalidToken) ' Compliant

            Dim builder1 As JwtBuilder = New JwtBuilder().WithSecret(Secret)
            builder1.Decode(InvalidToken) ' Noncompliant

            Dim builder2 As JwtBuilder = builder1.MustVerifySignature()
            builder2.Decode(InvalidToken) ' Compliant

            Dim builder3 As JwtBuilder = New JwtBuilder().WithSecret(Secret).MustVerifySignature()
            builder3.Decode(InvalidToken) ' Compliant
        End Sub

    End Class

End Namespace
