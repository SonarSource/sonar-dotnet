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

        // Encoding with JWT.NET is safe

        void DecodingWithDecoder(JwtDecoder decoder)
        {
            var decoded1 = decoder.Decode(invalidToken, secret, true);// Compliant
            var decoded2 = decoder.Decode(invalidToken, secret, false); // Noncompliant {{Use only strong cipher algorithms when verifying the signature of this JWT.}}

            var decoded3 = decoder.Decode(invalidToken, secret, verify: true);// Compliant
            var decoded4 = decoder.Decode(invalidToken, secret, verify: false); // Noncompliant

            var decoded5 = decoder.Decode(invalidToken, secret, verify: true);// Compliant
            var decoded6 = decoder.Decode(invalidToken, secret, verify: false); // Noncompliant

            var decoded7 = decoder.Decode(invalidToken, verify: true, key: secret);// Compliant
            var decoded8 = decoder.Decode(invalidToken, verify: false, key: secret); // Noncompliant

            var decoded9 = decoder.Decode(invalidToken, verify: true, key: new byte[] { 42 });// Compliant
            var decoded10 = decoder.Decode(invalidToken, verify: false, key: new byte[] { 42 }); // Noncompliant

            var decoded11 = decoder.Decode(invalidToken); // Noncompliant
            var decoded12 = decoder.Decode(invalidParts); // Noncompliant

            var decoded21 = decoder.DecodeToObject(invalidToken, secret, true); // Compliant
            var decoded22 = decoder.DecodeToObject(invalidToken, secret, false); // Noncompliant

            var decoded31 = decoder.DecodeToObject<UserInfo>(invalidToken, secret, true); // Compliant
            var decoded32 = decoder.DecodeToObject<UserInfo>(invalidToken, secret, false); // Noncompliant
        }

        void DecodingWithCustomDecoder(CustomDecoder decoder)
        {
            var decoded1 = decoder.Decode(invalidToken, secret, true);// Compliant
            var decoded2 = decoder.Decode(invalidToken, secret, false); // Noncompliant {{Use only strong cipher algorithms when verifying the signature of this JWT.}}

            var decoded3 = decoder.Decode(invalidToken, secret, verify: true);// Compliant
            var decoded4 = decoder.Decode(invalidToken, secret, verify: false); // Noncompliant

            var decoded5 = decoder.Decode(invalidToken, secret, verify: true);// Compliant
            var decoded6 = decoder.Decode(invalidToken, secret, verify: false); // Noncompliant

            var decoded7 = decoder.Decode(invalidToken, verify: true, key: secret);// Compliant
            var decoded8 = decoder.Decode(invalidToken, verify: false, key: secret); // Noncompliant

            var decoded9 = decoder.Decode(invalidToken, verify: true, key: new byte[] { 42 });// Compliant
            var decoded10 = decoder.Decode(invalidToken, verify: false, key: new byte[] { 42 }); // Noncompliant

            var decoded11 = decoder.Decode(invalidToken); // Noncompliant
            var decoded12 = decoder.Decode(invalidParts); // Noncompliant

            var decoded21 = decoder.DecodeToObject(invalidToken, secret, true); // Compliant
            var decoded22 = decoder.DecodeToObject(invalidToken, secret, false); // Noncompliant

            var decoded31 = decoder.DecodeToObject<UserInfo>(invalidToken, secret, true); // Compliant
            var decoded32 = decoder.DecodeToObject<UserInfo>(invalidToken, secret, false); // Noncompliant
        }

        void DecodingWithBuilder()
        {
            var decoded1 = new JwtBuilder()
              .WithSecret(secret)
              .Decode(invalidToken); // Noncompliant {{Use only strong cipher algorithms when verifying the signature of this JWT.}}

            var decoded2 = new JwtBuilder()
              .WithSecret(secret)
              .MustVerifySignature()
              .Decode(invalidToken); // Compliant

            var builder1 = new JwtBuilder().WithSecret(secret);
            builder1.Decode(invalidToken); // Noncompliant

            var builder2 = builder1.MustVerifySignature();
            builder2.Decode(invalidToken); // Compliant

            var builder3 = new JwtBuilder().WithSecret(secret).MustVerifySignature();
            builder3.Decode(invalidToken); // Compliant

            var decoded11 = new JwtBuilder()
                .WithSecret(secret)
                .WithVerifySignature(true)
                .MustVerifySignature()
                .DoNotVerifySignature()
                .Decode(invalidToken); // Noncompliant

            var Decoded12 = new JwtBuilder()
                .WithSecret(secret)
                .WithVerifySignature(false)
                .DoNotVerifySignature()
                .MustVerifySignature()
                .Decode(invalidToken); // Compliant

            var Decoded21 = new JwtBuilder()
                .WithSecret(secret)
                .DoNotVerifySignature()
                .WithVerifySignature(false)
                .WithVerifySignature(true)
                .Decode(invalidToken); // Compliant

            var Decoded31 = new JwtBuilder()
                .WithSecret(secret)
                .MustVerifySignature()
                .WithVerifySignature(true)
                .WithVerifySignature(false)
                .Decode(invalidToken); // Noncompliant
        }
    }

    class UserInfo
    {
        string Name { get; set; }
    }

    class CustomDecoder : IJwtDecoder
    {
        public string Decode(JwtParts jwt)
        {
            throw new NotImplementedException();
        }

        public string Decode(string token)
        {
            throw new NotImplementedException();
        }

        public string Decode(string token, string key, bool verify)
        {
            throw new NotImplementedException();
        }

        public string Decode(string token, string[] keys, bool verify)
        {
            throw new NotImplementedException();
        }

        public string Decode(string token, byte[] key, bool verify)
        {
            throw new NotImplementedException();
        }

        public string Decode(string token, byte[][] keys, bool verify)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> DecodeToObject(string token)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> DecodeToObject(string token, string key, bool verify)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> DecodeToObject(string token, string[] keys, bool verify)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> DecodeToObject(string token, byte[] key, bool verify)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, object> DecodeToObject(string token, byte[][] keys, bool verify)
        {
            throw new NotImplementedException();
        }

        public T DecodeToObject<T>(string token)
        {
            throw new NotImplementedException();
        }

        public T DecodeToObject<T>(string token, string key, bool verify)
        {
            throw new NotImplementedException();
        }

        public T DecodeToObject<T>(string token, string[] keys, bool verify)
        {
            throw new NotImplementedException();
        }

        public T DecodeToObject<T>(string token, byte[] key, bool verify)
        {
            throw new NotImplementedException();
        }

        public T DecodeToObject<T>(string token, byte[][] keys, bool verify)
        {
            throw new NotImplementedException();
        }
    }
}
