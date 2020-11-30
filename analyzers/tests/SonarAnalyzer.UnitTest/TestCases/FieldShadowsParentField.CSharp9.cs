public record Fruit
{
    protected int ripe;
    protected static int leafs;
}

public record Raspberry : Fruit
{
    private bool ripe;  // Noncompliant {{'ripe' is the name of a field in 'Fruit'.}}
    protected static int leafs; // Compliant, static is ignored
}
