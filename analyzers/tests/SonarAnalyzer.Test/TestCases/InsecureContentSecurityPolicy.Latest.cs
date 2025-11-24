using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

public class InsecureContentSecurityPolicy
{
    public void CustomDictionary(MyHeaderDictionary myheaderDictionary)
    {
        myheaderDictionary.Add("Content-Security-Policy", "script-src 'self';");                    // Compliant
        myheaderDictionary.Add("Content-Security-Policy", "script-src 'self' 'unsafe-inline';");    // Noncompliant
        myheaderDictionary.Append("Content-Security-Policy", "script-src 'self';");                 // Compliant
        myheaderDictionary.Append("Content-Security-Policy", "script-src 'self' 'unsafe-inline';"); // Compliant
        myheaderDictionary["Content-Security-Policy"] = "script-src 'self';";                       // Compliant
        myheaderDictionary["Content-Security-Policy"] = "script-src 'self' 'unsafe-inline';";       // Compliant
    }

    public void ConditionalNullAssignment(HttpContext context)
    {
        context?.Response.Headers.ContentSecurityPolicy = "*";  // Noncompliant
        context.Response?.Headers.ContentSecurityPolicy = "*";  // Noncompliant
        context.Response.Headers?.ContentSecurityPolicy = "*";  // Noncompliant
                                                                // Noncompliant@-1 FP, reported twice

        context?.Response.Headers["Content-Security-Policy"] = "script-src 'self' 'unsafe-inline';"; // Noncompliant
        context.Response?.Headers["Content-Security-Policy"] = "script-src 'self' 'unsafe-inline';"; // FN
        context.Response.Headers?["Content-Security-Policy"] = "script-src 'self' 'unsafe-inline';"; // FN
    }
}

public class MyHeaderDictionary : IHeaderDictionary
{
    public long? ContentLength { get; set; }
    public ICollection<string> Keys => throw new NotImplementedException();
    public ICollection<StringValues> Values => throw new NotImplementedException();
    public int Count => throw new NotImplementedException();
    public bool IsReadOnly => throw new NotImplementedException();
    public StringValues this[string key]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public void Add(string key, StringValues value) { }
    public void Append(string key, StringValues value) { }
    public bool ContainsKey(string key) => throw new NotImplementedException();
    public bool Remove(string key) => throw new NotImplementedException();
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out StringValues value) => throw new NotImplementedException();
    public void Clear() => throw new NotImplementedException();
    public bool Contains(KeyValuePair<string, StringValues> item) => throw new NotImplementedException();
    public void CopyTo(KeyValuePair<string, StringValues>[] array, int arrayIndex) => throw new NotImplementedException();
    public bool Remove(KeyValuePair<string, StringValues> item) => throw new NotImplementedException();
    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator() => throw new NotImplementedException();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    public void Add(KeyValuePair<string, StringValues> item) => throw new NotImplementedException();
}
