using System;
using System.Collections.Generic;
using System.Security.Cryptography;

public class Sample
{
    public void Examples()
    {
        AesCng aes = new AesCng();
        aes.CreateEncryptor();
        int a;
        (var rgb, a) = (new byte[16], 42);
        aes.CreateEncryptor(aes.Key, rgb); // FN
    }
}
