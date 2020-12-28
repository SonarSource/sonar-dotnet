using System;
using System.Collections.Generic;
using JWT;
using JWT.Algorithms;
using JWT.Builder;

namespace Tests.Diagnostics
{
    class Program
    {
        const string secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";
        const string invalidToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJmb28iOiJmYWtlYmFyIiwiaWF0IjoxNTc1NjQ0NTc3fQ.pcX_7snpSGf01uBfaM8XPkbgdhs1gq9JcYRCQvZrJyk";

        private JwtParts invalidParts;

        void ExtensionCalledOnInstance(JwtDecoder decoder)
        {
            var decoded = decoder.Decode(invalidToken); // Noncompliant
            decoded = decoder.Decode(invalidParts); // Noncompliant

            decoded = decoder.Decode(invalidToken, new byte[] { 42 }, false); // Noncompliant
            decoded = decoder.Decode(invalidToken, new byte[] { 42 }, true); // Compliant

            decoded = decoder.Decode(invalidToken, secret, false); // Noncompliant
            decoded = decoder.Decode(invalidToken, secret, true); // Compliant

            var decodedObject = decoder.DecodeToObject<object>(invalidToken, new byte[] { 42 }, false); // Noncompliant
            decodedObject = decoder.DecodeToObject<object>(invalidToken, new byte[] { 42 }, true); // Compliant

            decodedObject = decoder.DecodeToObject<object>(invalidToken, secret, false); // Noncompliant
            decodedObject = decoder.DecodeToObject<object>(invalidToken, secret, true); // Compliant

            decodedObject = decoder.DecodeToObject<object>(invalidToken); // Noncompliant
            IDictionary<string, object> decodedMap = decoder.DecodeToObject(invalidToken); // Noncompliant
        }

        void ExtensionCalledThroughStaticClass(JwtDecoder decoder)
        {
            var decoded = JwtDecoderExtensions.Decode(decoder, invalidToken); // Noncompliant
            decoded = JwtDecoderExtensions.Decode(decoder, invalidParts); // Noncompliant

            decoded = JwtDecoderExtensions.Decode(decoder, invalidToken, new byte[] { 42 }, false); // Noncompliant
            decoded = JwtDecoderExtensions.Decode(decoder, invalidToken, new byte[] { 42 }, true); // Compliant

            decoded = JwtDecoderExtensions.Decode(decoder, invalidToken, secret, false); // Noncompliant
            decoded = JwtDecoderExtensions.Decode(decoder, invalidToken, secret, true); // Compliant

            var decodedObject = JwtDecoderExtensions.DecodeToObject<object>(decoder, invalidToken, new byte[] { 42 }, false); // Noncompliant
            decodedObject = JwtDecoderExtensions.DecodeToObject<object>(decoder, invalidToken, new byte[] { 42 }, true); // Compliant

            decodedObject = JwtDecoderExtensions.DecodeToObject<object>(decoder, invalidToken, secret, false); // Noncompliant
            decodedObject = JwtDecoderExtensions.DecodeToObject<object>(decoder, invalidToken, secret, true); // Compliant

            decodedObject = JwtDecoderExtensions.DecodeToObject<object>(decoder, invalidToken); // Noncompliant
            IDictionary<string, object> decodedMap = JwtDecoderExtensions.DecodeToObject(decoder, invalidToken); // Noncompliant
        }
    }
}
