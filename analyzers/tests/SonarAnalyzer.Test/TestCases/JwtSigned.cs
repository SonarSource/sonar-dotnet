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
        private bool trueField = true;
        private bool falseField = false;

        // Encoding with JWT.NET is safe

        void DecodingWithDecoder(JwtDecoder decoder)
        {
            string decoded;
            decoded = decoder.Decode(invalidToken, secret, true);
            decoded = decoder.Decode(invalidToken, secret, false); // Noncompliant {{Use only strong cipher algorithms when verifying the signature of this JWT.}}
            decoded = decoder?.Decode(invalidToken, secret, false); // Noncompliant

            decoded = decoder.Decode(invalidToken, secret, trueField);
            decoded = decoder.Decode(invalidToken, secret, falseField);         // Noncompliant

            decoded = decoder.Decode(invalidToken, secret, verify: true);
            decoded = decoder.Decode(invalidToken, secret, verify: false);      // Noncompliant

            decoded = decoder.Decode(invalidToken, secret, verify: true);
            decoded = decoder.Decode(invalidToken, secret, verify: false);      // Noncompliant

            decoded = decoder.Decode(invalidToken, verify: true, key: secret);
            decoded = decoder.Decode(invalidToken, verify: false, key: secret); // Noncompliant

            decoded = decoder.Decode(invalidToken, verify: true, key: new byte[] { 42 });
            decoded = decoder.Decode(invalidToken, verify: false, key: new byte[] { 42 }); // Noncompliant

            decoded = decoder.Decode(invalidToken); // Noncompliant
            decoded = decoder.Decode(invalidParts); // Noncompliant

            var decoded21 = decoder.DecodeToObject(invalidToken, secret, true);
            var decoded22 = decoder.DecodeToObject(invalidToken, secret, false); // Noncompliant

            var decoded31 = decoder.DecodeToObject<UserInfo>(invalidToken, secret, true);
            var decoded32 = decoder.DecodeToObject<UserInfo>(invalidToken, secret, false); // Noncompliant

            // Reassigned
            trueField = false;
            falseField = true;
            decoded = decoder.Decode(invalidToken, secret, trueField);          // Noncompliant
            decoded = decoder.Decode(invalidToken, secret, falseField);
        }

        void DecodingWithCustomDecoder(CustomDecoder decoder)
        {
            var decoded1 = decoder.Decode(invalidToken, secret, true);
            var decoded2 = decoder.Decode(invalidToken, secret, false); // Noncompliant {{Use only strong cipher algorithms when verifying the signature of this JWT.}}

            var decoded3 = decoder.Decode(invalidToken, secret, verify: true);
            var decoded4 = decoder.Decode(invalidToken, secret, verify: false); // Noncompliant

            var decoded5 = decoder.Decode(invalidToken, secret, verify: true);
            var decoded6 = decoder.Decode(invalidToken, secret, verify: false); // Noncompliant

            var decoded7 = decoder.Decode(invalidToken, verify: true, key: secret);
            var decoded8 = decoder.Decode(invalidToken, verify: false, key: secret); // Noncompliant

            var decoded9 = decoder.Decode(invalidToken, verify: true, key: new byte[] { 42 });
            var decoded10 = decoder.Decode(invalidToken, verify: false, key: new byte[] { 42 }); // Noncompliant

            var decoded11 = decoder.Decode(invalidToken); // Noncompliant
            var decoded12 = decoder.Decode(invalidParts); // Noncompliant

            var decoded21 = decoder.DecodeToObject(invalidToken, secret, true);
            var decoded22 = decoder.DecodeToObject(invalidToken, secret, false); // Noncompliant

            var decoded31 = decoder.DecodeToObject<UserInfo>(invalidToken, secret, true);
            var decoded32 = decoder.DecodeToObject<UserInfo>(invalidToken, secret, false); // Noncompliant
        }

        void DecodingWithBuilder()
        {
            var decoded1 = new JwtBuilder() // Noncompliant {{Use only strong cipher algorithms when verifying the signature of this JWT.}}
              .WithSecret(secret)
              .Decode(invalidToken);

            var decoded2 = new JwtBuilder()
              .WithSecret(secret)
              .MustVerifySignature()
              .Decode(invalidToken);

            var builder1 = new JwtBuilder().WithSecret(secret);
            builder1.Decode(invalidToken); // Noncompliant

            try
            {
                if (true)
                {
                    builder1.Decode(invalidToken); // Noncompliant, tracking outside nested block
                }
            }
            finally
            {
            }

            var builder2 = builder1.MustVerifySignature();
            builder2.Decode(invalidToken);

            var builder3 = new JwtBuilder().WithSecret(secret).MustVerifySignature();
            builder3.Decode(invalidToken);

            var builder4 = (((new JwtBuilder()).WithSecret(secret)));
            builder4.Decode(invalidToken); // Noncompliant

            var builder5 = new JwtBuilder().WithSecret(secret).DoNotVerifySignature();
            builder5.Decode(invalidToken); // Noncompliant

            var decoded11 = new JwtBuilder()  // Noncompliant
                .WithSecret(secret)
                .WithVerifySignature(true)
                .MustVerifySignature()
                .DoNotVerifySignature()
                .Decode(invalidToken);

            var Decoded12 = new JwtBuilder()
                .WithSecret(secret)
                .WithVerifySignature(false)
                .DoNotVerifySignature()
                .MustVerifySignature()
                .Decode(invalidToken);

            var Decoded21 = new JwtBuilder()
                .WithSecret(secret)
                .DoNotVerifySignature()
                .WithVerifySignature(false)
                .WithVerifySignature(true)
                .Decode(invalidToken);

            var Decoded31 = new JwtBuilder()  // Noncompliant
                .WithSecret(secret)
                .MustVerifySignature()
                .WithVerifySignature(true)
                .WithVerifySignature(false)
                .Decode(invalidToken);
        }

        void DecodingWithBuilder_FPs(bool condition)
        {
            var builder1 = new JwtBuilder();
            Init();
            builder1.Decode(invalidToken); // Noncompliant FP, initialization in local function is not tracked

            void Init()
            {
                builder1 = builder1.WithSecret(secret).MustVerifySignature();
            }
        }

        void DecodingWithBuilder_FNs(bool condition)
        {
            var builder1 = new JwtBuilder();
            if (condition)
            {
                builder1 = builder1.WithSecret(secret);
            }
            builder1.Decode(invalidToken); // FN, this is not SE rule, only linear initialization is considered

            CreateBuilder().Decode(invalidToken); // FN, cross procedural initialization is not tracked
            CreateLocalBuilder().Decode(invalidToken); // FN, local function initialization is not tracked

            JwtBuilder CreateLocalBuilder() => new JwtBuilder().DoNotVerifySignature();
        }

        JwtBuilder CreateBuilder()
        {
            return new JwtBuilder().DoNotVerifySignature();
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
