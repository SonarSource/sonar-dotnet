namespace CSharp15
{
    public class Cat { }
    public class Dog { }

    public union my_pet(Cat, Dog);              // Noncompliant {{Rename union 'my_pet' to match pascal case naming rules, consider using 'Mypet'.}}
    public union Pet(Cat, Dog);                 // Compliant
    public union _Pet(Cat, Dog);                // Noncompliant {{Rename union '_Pet' to match pascal case naming rules, trim underscores from the name.}}
    public closed class my_closed { }           // Noncompliant
}
