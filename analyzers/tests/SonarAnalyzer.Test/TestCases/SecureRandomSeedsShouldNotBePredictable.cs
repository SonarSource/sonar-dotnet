using System;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Crypto.Digests;

class Testcases
{
    const int SEED = 42;

    void SecureRandom_Baseline(bool condition, byte b, SecureRandom sr2)
    {
        var sr = SecureRandom.GetInstance("SHA256PRNG", true);
        sr.Next();                  // Compliant, autoseed is true.

        sr = SecureRandom.GetInstance("SHA256PRNG", condition);
        sr.Next();                  // Compliant, autoSeed is unknown.

        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next();                  // Noncompliant {{Set an unpredictable seed before generating random values.}}
    //  ^^^^^^^^^

        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.SetSeed(SEED);
        sr.Next();                  // Noncompliant {{Set an unpredictable seed before generating random values.}}
    //  ^^^^^^^^^

        sr = sr2;
        sr.Next();                  // Compliant
    }

    void SecureRandom_Conditional(byte[] bs, bool condition)
    {
        // IF-THEN-ELSE
        var sr = SecureRandom.GetInstance("SHA256PRNG", false);
        if (condition)
        {
            sr.Next(); // Noncompliant
            sr.SetSeed(bs);
            sr.Next(); // Compliant
        }
        else
        {
            sr.Next(); // Noncompliant
            sr.SetSeed(SEED);
            sr.Next(); // Noncompliant
        }
        sr.Next(); // Noncompliant

        // IF-TRUE-ELSE
        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        if (true)
        {
            sr.Next(); // Noncompliant
            sr.SetSeed(bs);
            sr.Next(); // Compliant
        }
        else
        {
            sr.Next(); // Compliant, dead code
        }
        sr.Next(); // Compliant

        // IF-FALSE
        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        if (false)
        {
            sr.Next(); // Compliant
            sr.SetSeed(bs);
            sr.Next(); // Compliant
        }
        sr.Next(); // Noncompliant

        // TERNARY
        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        sr.SetSeed(condition ? bs : new byte[42]);
        sr.Next(); // Noncompliant

        // TERNARY-ALWAYS-TRUE
        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        sr.SetSeed(true ? new byte[42] : bs);
        sr.Next(); // Noncompliant
        sr.SetSeed(true ? bs : new byte[42]);
        sr.Next(); // Compliant

        // TERNARY-ALWAYS-FALSE
        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        sr.SetSeed(false ? bs : new byte[42]);
        sr.Next(); // Noncompliant
        sr.SetSeed(false ? new byte[42] : bs);
        sr.Next(); // Compliant

        // SWITCH EXPRESSION
        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        sr.SetSeed(condition switch { true => bs, false => new byte[42] });
        sr.Next(); // Noncompliant

        // SWITCH EXPRESSION ALWAYS TRUE
        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        sr.SetSeed(true switch { true => bs, false => new byte[42] });
        sr.Next(); // Compliant

        // SWITCH STATEMENT
        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        switch (condition)
        {
            case true:
                sr.SetSeed(bs);
                sr.Next(); // Compliant
                break;
            case false:
                sr.SetSeed(new byte[42]);
                sr.Next(); // Noncompliant
                break;
        }
        sr.Next(); // Noncompliant
    }

    void SecureRandom_Loop(byte[] bs, bool condition)
    {
        // FOR-LOOP
        var sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        for (int i = 0; i < 42; i++)
        {
            sr.SetSeed(bs);
        }
        sr.Next(); // Compliant

        // FOR-LOOP DEAD CODE
        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        for (int i = 0; i < 0; i++)
        {
            sr.SetSeed(bs); // Dead code
        }
        sr.Next(); // Noncompliant

        // WHILE LOOP
        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        while (condition)
        {
            sr.Next(); // Noncompliant
            sr.SetSeed(bs);
        }
        sr.Next(); // Noncompliant

        // GOTO
        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        if (condition)
        {
            sr.SetSeed(bs);
            goto Good;
        }
        else
        {
            goto Bad;
        }

    Good:
        sr.Next(); // Compliant
    Bad:
        sr.Next(); // Noncompliant
    }

    void SecureRandom_TryCatchFinally(byte[] bs, bool condition)
    {
        // TRY-CATCH
        var sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        try
        {
            sr.SetSeed(bs);
        }
        catch (Exception)
        {
            sr.Next(); // Noncompliant
        }
        // TRY-FINALLY
        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        try
        {
            sr.SetSeed(bs);
        }
        finally
        {
            sr.Next(); // Noncompliant
        }
        // TRY-CATCH-FINALLY
        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        try
        {
            sr.SetSeed(bs);
        }
        catch (Exception)
        {
            sr.Next(); // Noncompliant
        }
        finally
        {
            sr.Next(); // Noncompliant
        }
    }

    void SecureRandom_Arrays(byte b, char[] whoKnows)
    {
        var xs = new int[] { 1, 2, 3 };
        var bs = new byte[] { 1, 2, 3 };
        var cs = new char[] { '1', '2', '3' };

        var sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant

        sr.SetSeed(bs);
        sr.Next(); // Noncompliant

        _ = bs[42];
        sr.SetSeed(bs);
        sr.Next(); // Noncompliant

        bs[42] = b;
        sr.SetSeed(bs);
        sr.Next(); // Compliant

        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant

        sr.SetSeed(Encoding.UTF8.GetBytes(cs));
        sr.Next(); // Noncompliant

        sr.SetSeed(Convert.FromBase64CharArray(cs, 0, 42));
        sr.Next(); // Noncompliant

        cs[42] = 'W';
        sr.SetSeed(Convert.FromBase64CharArray(cs, 0, 42));
        sr.Next(); // Compliant FN, array is still constant

        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        sr.SetSeed(Encoding.UTF8.GetBytes(whoKnows));
        sr.Next(); // Compliant

        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        sr.SetSeed(Convert.FromBase64CharArray(whoKnows, 0, 42));
        sr.Next(); // Compliant

        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        var ss = new byte[3];
        ss[0] = b;
        ss.Initialize();
        sr.SetSeed(ss);
        sr.Next(); // Noncompliant, Initialize is predictable

        sr = SecureRandom.GetInstance("SHA256PRNG", false);
        sr.Next(); // Noncompliant
        sr.SetSeed(xs.Cast<byte>().ToArray());
        sr.Next(); // Compliant FN, array is predictable
    }

    void FromArguments(SecureRandom sr1, SecureRandom sr2, SecureRandom sr3, VmpcRandomGenerator rng1, DigestRandomGenerator rng2, int seed, byte[] bs)
    {
        sr1.Next(); // Compliant

        sr2.SetSeed(SEED);
        sr2.Next(); // Compliant

        sr3.SetSeed(seed);
        sr3.Next(); // Compliant

        rng1.NextBytes(bs); // Compliant
        rng1.AddSeedMaterial(SEED);
        rng1.NextBytes(bs); // Compliant

        rng2.NextBytes(bs); // Compliant
        rng2.AddSeedMaterial(seed);
        rng2.NextBytes(bs); // Compliant

        var mySr = new SecureRandom(rng1);
        mySr.Next(); // Compliant

        mySr = new SecureRandom(rng1, 5);
        mySr.Next(); // Compliant

        mySr = new SecureRandom(rng1, 20);
        mySr.Next(); // Compliant

        mySr = new SecureRandom(rng2);
        mySr.Next(); // Compliant

        mySr = new SecureRandom(rng2, 5);
        mySr.Next(); // Compliant

        mySr = new SecureRandom(rng2, 20);
        mySr.Next(); // Compliant
    }

    void RandomGenerator_Inside_SecureRandom(byte[] bs, string s)
    {
        var notHardcoded = Encoding.UTF8.GetBytes(s);
        var hardcoded = Convert.FromBase64String("exploding whale");

        var generator = new DigestRandomGenerator(new Sha256Digest());
        var rng = new SecureRandom(generator);
        rng.NextBytes(bs); // Noncompliant

        generator = new DigestRandomGenerator(new Sha256Digest());
        rng = new SecureRandom(generator, 11);
        rng.NextBytes(bs); // Noncompliant

        generator = new DigestRandomGenerator(new Sha256Digest());
        generator.AddSeedMaterial(hardcoded);
        rng = new SecureRandom(generator);
        rng.NextBytes(bs); // Noncompliant

        generator = new DigestRandomGenerator(new Sha256Digest());
        rng = new SecureRandom(generator);
        rng.SetSeed(hardcoded);
        rng.NextBytes(bs); // Noncompliant

        generator = new DigestRandomGenerator(new Sha256Digest());
        generator.AddSeedMaterial(hardcoded);
        rng = new SecureRandom(generator);
        rng.SetSeed(hardcoded);
        rng.NextBytes(bs); // Noncompliant

        generator = new DigestRandomGenerator(new Sha256Digest());
        rng = new SecureRandom(generator, 16);
        rng.NextBytes(bs); // Compliant

        generator = new DigestRandomGenerator(new Sha256Digest());
        generator.AddSeedMaterial(notHardcoded);
        rng = new SecureRandom(generator);
        rng.SetSeed(hardcoded);
        rng.NextBytes(bs); // Compliant

        generator = new DigestRandomGenerator(new Sha256Digest());
        generator.AddSeedMaterial(hardcoded);
        rng = new SecureRandom(generator);
        rng.SetSeed(notHardcoded);
        rng.NextBytes(bs); // Compliant

        rng = new SecureRandom();
        rng.NextBytes(bs); // Compliant

        generator = new DigestRandomGenerator(new Sha256Digest());
        rng = new SecureRandom(generator);
        generator.AddSeedMaterial(notHardcoded);
        rng.Next(); // Noncompliant FP, we cannot infer that "rng" is safe after the constructor

        generator = new DigestRandomGenerator(new Sha256Digest());
        rng = new SecureRandom(generator);
        rng.SetSeed(notHardcoded);
        generator.NextBytes(bs); // Noncompliant FP, we cannot infer that "generator" is safe after the constructor
    }
}
