using System;
using System.Collections.Generic;

class TestCase
{
    private string[] stringArray = new string[10];

    public string[] Property1
    {
        get { return [..stringArray]; } // FN
    }

    public string[][] Property2
    {
        get { return [stringArray]; } // FN
    }
}
