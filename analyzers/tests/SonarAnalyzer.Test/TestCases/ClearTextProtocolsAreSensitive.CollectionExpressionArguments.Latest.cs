using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharp15
{
    public class UrlCollection : IEnumerable<string>
    {
        public UrlCollection(string endpoint) { }

        public void Add(string item) { }

        public IEnumerator<string> GetEnumerator() => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static UrlCollection Create() => [with("http://sonar.com"), "https://sonar.com"]; // Noncompliant NET-4895
    }
}
