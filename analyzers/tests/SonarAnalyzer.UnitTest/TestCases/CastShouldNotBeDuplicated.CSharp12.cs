using System.Collections.Generic;
using Person = (string name, string surname);

class MyClass
{
    void AliasType()
    {
        var persons = new List<object> { ("Mickey", "Mouse") };
        if (persons[0] is Person person)
        {
            var a = (Person)person; // FN
        }
    }
}
