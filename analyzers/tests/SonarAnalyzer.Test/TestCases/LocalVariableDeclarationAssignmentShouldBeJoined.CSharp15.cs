using System.Collections.Generic;

class Cat { }
class Dog { }
union Pet(Cat, Dog);

class Sample
{
    void M()
    {
        List<int> list; // Noncompliant
        list = [with(capacity: 4), 1];

        Pet pet; // Noncompliant
        pet = new Dog();
    }
}
