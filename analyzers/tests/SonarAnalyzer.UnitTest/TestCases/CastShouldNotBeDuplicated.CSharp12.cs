using System.Collections.Generic;
using Person = (string name, string surname);

class MyClass
{
    void AliasType(Person person)
    {
        var a = (Person)person; // FN
    }
}
