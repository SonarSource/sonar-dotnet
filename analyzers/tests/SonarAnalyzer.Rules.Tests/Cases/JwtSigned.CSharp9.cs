using JWT.Builder;

const string secret = "GQDstcKsx0NHjPOuXOYg5MbeJ1XT0uFiwDVvVBrk";
const string invalidToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJmb28iOiJmYWtlYmFyIiwiaWF0IjoxNTc1NjQ0NTc3fQ.pcX_7snpSGf01uBfaM8XPkbgdhs1gq9JcYRCQvZrJyk";

JwtBuilder decoded1 = new(); // FN {{Use only strong cipher algorithms when verifying the signature of this JWT.}}
decoded1.WithSecret(secret).Decode(invalidToken);

JwtBuilder decoded2 = new();
decoded2.WithSecret(secret).MustVerifySignature().Decode(invalidToken); // Compliant
