int x = 5;
int y = 6;

y = 5;

y = 5;

y = 5;

y = y switch
{
    5 => 5, // Fixed
    6 => 6  // Fixed
};

y = y switch
{
    not 5 when x == 5 => 5
};

y = y switch
{
    5 => 6
};

x = y switch
{
    5 => 6
};

y = y switch
{
    4 => 4,      // Fixed
    not 5 => 5,
};

y = 5;

y = y switch
{
    5 => 6,
    _ => y
};

y = y;

y = y switch
{
    4 => 4,      // Fixed
    not x => 5,  // Error [CS0150]
};

int z = y switch
{
    not 5 => 5
};

if ((x,y) is (1,2))   // Compliant FN
{
    x = 1;
    y = 2;
}

SomeClass someClass = new SomeClass() { SomeField = 42 };

if (someClass.SomeField == 42) // FN
{
    someClass.SomeField = 42;
}

if (someClass is { SomeField: 42 }) // FN
{
    someClass.SomeField = 42;
}

public class SomeClass
{
    public int SomeField;
}

record Record
{
    string x;

    string CompliantProperty1
    {
        init
        {
            if (x != value)
            {
                x = value;
            }
        }
    }

    string CompliantProperty2
    {
        init { x = value; }
    }
}
