using System;
using JWT;
using JWT.Algorithms;
using JWT.Builder;

namespace Tests.Diagnostics
{
    class Program
    {
        const string secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";
        const string invalidToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJmb28iOiJmYWtlYmFyIiwiaWF0IjoxNTc1NjQ0NTc3fQ.pcX_7snpSGf01uBfaM8XPkbgdhs1gq9JcYRCQvZrJyk";

        private IJwtAlgorithm algorithm = new HMACSHA256Algorithm();

        // Encoding with JWT.NET is safe

        void DecodingWithDecoder()
        {
            var decoder = new JwtDecoder(null, null, null, algorithm);

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
        }
    }
}
