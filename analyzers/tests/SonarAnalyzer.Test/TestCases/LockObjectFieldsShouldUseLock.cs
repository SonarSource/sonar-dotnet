class LegacyLocking
{
    private readonly object gate = new();    // Compliant, System.Threading.Lock requires C# 13

    void Execute(object candidate)
    {
        lock (gate) { }
        lock (candidate) { }
    }
}
