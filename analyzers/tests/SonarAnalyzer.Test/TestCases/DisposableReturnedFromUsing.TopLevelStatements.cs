using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

FileStream WriteToFile(string path, string text)
{
    using (var fs = File.Create(path)) // Noncompliant {{Remove the 'using' statement; it will cause automatic disposal of 'fs'.}}
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        fs.Write(bytes, 0, bytes.Length);
        return fs;
    }
}

FileStream WriteToFile4(string text)
{
    var f = new Func<FileStream>(static () =>
    {
        using var fs = File.Create(""); // Noncompliant
        return fs;
    });
    var fs = f();
    var bytes = Encoding.UTF8.GetBytes(text);
    fs.Write(bytes, 0, bytes.Length);
    return fs;
}

FileStream Method(string x, object y)
{
    using var fs1 = File.Create(x);
    var result = y switch
    {
        > 0 and < 10 => fs1,
        not null => null
    };
    return result; // FN, we don't track aliasing
}

FileStream TargetTypedNew()
{
    using FileStream fs1 = new(@"c:\foo.txt", FileMode.Open); // Noncompliant
    return fs1;
}

FileStream TargetTypedNew2()
{
    using (FileStream fs1 = new(@"c:\foo.txt", FileMode.Open)) // Noncompliant
    {
        return fs1;
    }
}
