using System;
using System.Collections.Generic;

class CSharp11
{
    Dictionary<string, int> dictionary = new();

    void OverwriteDictionary()
    {
        dictionary["""a"""] = 1; // Secondary {{The index/key set here gets set again later.}}
        dictionary["""a"""] = 2; // Noncompliant
    }

    void OverwriteArray()
    {
        string[] array = new string[2];
        array[0] = """a"""; // Secondary  {{The index/key set here gets set again later.}}
        array[1] = """b""";
        array[0] = """c"""; // Noncompliant
    }

    void SameIndexOnArray(CustomIndexer obj)
    {
        obj["""foo"""] = 42; // Compliant, not a collection or dictionary
        obj["""foo"""] = 42; // Compliant, not a collection or dictionary
    }

    public class CustomIndexer
    {
        public int this[string key]
        {
            get { return 1; }
            set { }
        }
    }
}

class CSharp13
{
    OrderedDictionary<string, int> orderedDictionary = new();

    void NewCollectionTypes()
    {
        orderedDictionary["""a"""] = 1; // Secondary
        orderedDictionary["""a"""] = 2; // Noncompliant
    }
}
