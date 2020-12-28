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

        Sub ExtensionCalledOnInstance(Decoder As JwtDecoder)

            Dim byteArray As Byte() = { 16, 23 }

            Dim Decoded As String = Decoder.Decode(invalidToken) ' Noncompliant
            Decoded = Decoder.Decode(invalidParts) ' Noncompliant

            Decoded = Decoder.Decode(invalidToken, byteArray, false) ' Noncompliant
            Decoded = Decoder.Decode(invalidToken, byteArray, true) ' Compliant

            Decoded = Decoder.Decode(invalidToken, secret, false) ' Noncompliant
            Decoded = Decoder.Decode(invalidToken, secret, true) ' Compliant

            Dim DecodedObject As Object = Decoder.DecodeToObject(Of Object)(invalidToken, byteArray, false) ' Noncompliant
            DecodedObject = Decoder.DecodeToObject(Of Object)(invalidToken, byteArray, true) ' Compliant

            DecodedObject = Decoder.DecodeToObject(Of Object)(invalidToken, secret, false) ' Noncompliant
            DecodedObject = Decoder.DecodeToObject(Of Object)(invalidToken, secret, true) ' Compliant

            DecodedObject = Decoder.DecodeToObject(Of Object)(invalidToken) ' Noncompliant
            Dim DecodedMap As IDictionary(Of String, Object) = decoder.DecodeToObject(invalidToken) ' Noncompliant

        End Sub

        Sub ExtensionCalledThroughStaticClass(Decoder As JwtDecoder)

            Dim byteArray As Byte() = { 16, 23 }

            Dim Decoded As String = JwtDecoderExtensions.Decode(Decoder, invalidToken) ' Noncompliant
            Decoded = JwtDecoderExtensions.Decode(Decoder, invalidParts) ' Noncompliant

            Decoded = JwtDecoderExtensions.Decode(Decoder, invalidToken, byteArray, false) ' Noncompliant
            Decoded = JwtDecoderExtensions.Decode(Decoder, invalidToken, byteArray, true) ' Compliant

            Decoded = JwtDecoderExtensions.Decode(Decoder, invalidToken, secret, false) ' Noncompliant
            Decoded = JwtDecoderExtensions.Decode(Decoder, invalidToken, secret, true) ' Compliant

            Dim DecodedObject As Object = JwtDecoderExtensions.DecodeToObject(Of Object)(Decoder, invalidToken, byteArray, false) ' Noncompliant
            DecodedObject = JwtDecoderExtensions.DecodeToObject(Of Object)(Decoder, invalidToken, byteArray, true) ' Compliant

            DecodedObject = JwtDecoderExtensions.DecodeToObject(Of Object)(Decoder, invalidToken, secret, false) ' Noncompliant
            DecodedObject = JwtDecoderExtensions.DecodeToObject(Of Object)(Decoder, invalidToken, secret, true) ' Compliant

            DecodedObject = JwtDecoderExtensions.DecodeToObject(Of Object)(Decoder, invalidToken) ' Noncompliant
            Dim DecodedMap As IDictionary(Of String, Object) = JwtDecoderExtensions.DecodeToObject(Decoder, invalidToken) ' Noncompliant

        End Sub

    End Class

End Namespace
