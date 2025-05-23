﻿using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class TestCases
    {
        Dictionary<string, int> dictionary = new();

        void OverwriteDictionary()
        {
            dictionary["""a"""] = 1; // Secondary
            dictionary["""a"""] = 2; // Noncompliant
        }

        void OverwriteArray()
        {
            string[] array = new string[2];
            array[0] = """a"""; // Secondary
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
}
