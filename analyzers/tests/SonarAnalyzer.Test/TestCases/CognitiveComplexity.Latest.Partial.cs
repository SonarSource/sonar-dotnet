public partial class PartialProperty
{
    public partial int Property
    {
        get =>            // Noncompliant  {{Refactor this accessor to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
            true ? 0 : 1; // Secondary
        set { }
    }
}


public partial class PartialConstructor
{
    public partial PartialConstructor() // Noncompliant  {{Refactor this constructor to reduce its Cognitive Complexity from 1 to the 0 allowed.}} 
    {
        var x = true ? 1 : 0;           // Secondary
    }
}
