namespace TestReport;

[TestClass]
public sealed class Test1
{
    public enum Type : byte
    {
        Struct = 10,
        Structure = 10
    }

    [DataTestMethod]
    [DataRow(Type.Struct, "struct")]
    [DataRow(Type.Structure, "struct")]
    public void TestMethod1(Type type, string expected)
    {
    }
}