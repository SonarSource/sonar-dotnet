using System.Collections.Generic;
using Person = (string name, string surname);

class MyClass
{
    void AliasType(List<object> persons)
    {
        if (persons[0] is Person person)
        {
            var a = (Person)person; // FN
        }
    }
}
