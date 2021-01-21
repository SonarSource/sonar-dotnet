using System;

namespace Tests.TestCases
{
    public class Fruit
    {
        protected int ripe;
        protected static int leafs;
    }

    public class Raspberry : Fruit
    {
        private bool ripe;  // Noncompliant {{'ripe' is the name of a field in 'Fruit'.}}
//                   ^^^^
        protected static int leafs; // Compliant, static is ignored
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/3393
namespace Repro_3393
{
    public class Animal { }
    public class Dog : Animal { }

    public class AnimalContainer
    {
        protected readonly Animal animal;
    }

    public class DogContainer : AnimalContainer
    {
        private new readonly Dog animal; // Compliant, modifier "new" is used to explicitly declare the intention
    }
}
