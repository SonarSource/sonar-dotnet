bool condition = false;

if (condition) Method(); // Noncompliant
else
{
    Method();
}

int a, b = 10;
var i = 5; i = 6; i = 7; //Noncompliant

void Method() { }

record Record
{
    public Record()
    {
        int a, b = 10;
        var i = 5; i = 6; i = 7; //Noncompliant
    }
}
