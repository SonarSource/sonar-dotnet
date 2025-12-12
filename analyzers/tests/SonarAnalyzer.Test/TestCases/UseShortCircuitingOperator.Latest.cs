using System;
using System.Collections;
using System.Collections.Generic;

public class FieldKeyword
{
    public bool Prop
    {
        get
        {
            if (true | field)        // Noncompliant
            {
                return field | true; // Noncompliant
            }
            return false | field;    // Noncompliant
        }
    }
}

public class NullCondionalAssignment
{
    public class Sample
    {
        public bool Value { get; set; } 
    }

    public void Method(Sample input)
    {
        input?.Value = true | false; // Noncompliant
    }
}
